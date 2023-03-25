using System.Collections.Generic;
using System.Linq;

namespace SolarTracker.Services
{
    public class DriveService
    {
        private readonly StateProvider _stateProvider;
        private readonly DeviceSettings _deviceSettings;
        private readonly AppSettings _appSettings;
        private readonly IIoService _ioService;
        public DriveService(
            StateProvider stateProvider,
            DeviceSettings deviceSettings,
            AppSettings appSettings,
            IIoService ioService)
        {
            _stateProvider = stateProvider;
            _deviceSettings = deviceSettings;
            _appSettings = appSettings;
            _ioService = ioService;
        }

        public async Task<Orientation> DoStartupProcedure(CancellationToken token)
        {
            //maybe drive down
            if (!_ioService.AltitudeMinLimit)
                _ = await _ioService.Drive(DriveDirection.AltitudeNegative, TimeSpan.FromMinutes(2), token)
                    .ConfigureAwait(false);

            //maybe drive left
            if (!_ioService.AzimuthMinLimit)
                _ = await _ioService.Drive(DriveDirection.AzimuthNegative, TimeSpan.FromMinutes(2), token)
                    .ConfigureAwait(false);



            //drive up in 1 go (time for angle)
            var up = await _ioService.Drive(DriveDirection.AltitudePositive, TimeSpan.FromMinutes(2), token)
                .ConfigureAwait(false);
            if (!_ioService.AltitudeMaxLimit)
                throw new Exception("could not reach max altitude");
            //drive back in n go´s (time for start/stop)
            var down = new List<DriveResult>();
            while (!_ioService.AltitudeMinLimit && down.Count < 15)
            {
                var timeToDrive = up.TimeDriven.TotalSeconds / 5;
                var driven = await _ioService
                    .Drive(DriveDirection.AltitudeNegative, TimeSpan.FromSeconds(timeToDrive), token)
                    .ConfigureAwait(false);
                down.Add(driven);
            }
            if (!_ioService.AltitudeMinLimit)
                throw new Exception("could not reach min altitude");
            var altWasted = down.Sum(x => x.TimeDriven.TotalSeconds) - up.TimeDriven.TotalSeconds;
            _stateProvider.AltitudeWasteTime = Convert.ToSingle(altWasted / down.Count);
            _stateProvider.AltitudeDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAltitude - _deviceSettings.MinAltitude)
                                                                 / up.TimeDriven.TotalSeconds);



            //drive right in 1 go (time for angle)
            var right = await _ioService.Drive(DriveDirection.AzimuthPositive, TimeSpan.FromMinutes(2), token)
                .ConfigureAwait(false);
            if (!_ioService.AzimuthMaxLimit)
                throw new Exception("could not reach max azimuth");
            //drive back in n go´s (time for start/stop)
            var left = new List<DriveResult>();
            while (!_ioService.AzimuthMinLimit && left.Count < 30)
            {
                var timeToDrive = right.TimeDriven.TotalSeconds / 10;
                var driven = await _ioService
                    .Drive(DriveDirection.AzimuthNegative, TimeSpan.FromSeconds(timeToDrive), token)
                    .ConfigureAwait(false);
                left.Add(driven);
            }
            if (!_ioService.AzimuthMinLimit)
                throw new Exception("could not reach min azimuth");
            var aziWasted = left.Sum(x => x.TimeDriven.TotalSeconds) - right.TimeDriven.TotalSeconds;
            _stateProvider.AzimuthWasteTime = Convert.ToSingle(aziWasted / left.Count);
            _stateProvider.AzimuthDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAzimuth - _deviceSettings.MinAzimuth)
                                                                 / right.TimeDriven.TotalSeconds);

            //here we should know how much is the drive integration delay,
            //and how much angle do we cover per time.
            //Therefor we can calculate later how much time to drive....


            //set current position.
            var result = new Orientation(
                    _deviceSettings.MinAzimuth,
                    _deviceSettings.MinAltitude,
                    DateTime.Now.Add(_appSettings.AutoInterval));

            _stateProvider.CurrentOrientation = result;
            return result;
        }

        public async Task<Orientation> DriveToTarget(Orientation target, CancellationToken token)
        {
            var source = _stateProvider.CurrentOrientation ?? await DoStartupProcedure(token);

            var result = await DriveAzimuth(source, target, token);
            result = await DriveAltitude(result, target, token);

            _stateProvider.CurrentOrientation = result;
            return result;

            //special rules:
            //-when we are over or close to limits, we drive up to the limit
            //-we are saving drive results over the day
        }


        private async Task<Orientation> DriveAzimuth(Orientation source, Orientation target, CancellationToken token)
        {
            //turn differences in direction and time azimuth
            var direction = target.Azimuth > source.Azimuth
                ? DriveDirection.AzimuthPositive
                : DriveDirection.AzimuthNegative;
            //already in limit
            if (CheckLimit(direction))
            {
                return source with { ValidUntil = target.ValidUntil };
            }

            var driveAngle = Math.Abs(target.Azimuth - source.Azimuth);
            if (driveAngle < _appSettings.AzimuthMinAngleForDrive)
            {
                return source with { ValidUntil = target.ValidUntil };
            }


            var time = driveAngle / _stateProvider.AzimuthDegreePerSecond;
            var timeWithWaste = time + _stateProvider.AzimuthWasteTime;
            var driven = await _ioService.Drive(
                direction,
                TimeSpan.FromSeconds(timeWithWaste),
                token);


            var degreeDriven = Convert.ToSingle(_stateProvider.AzimuthDegreePerSecond * (driven.TimeDriven.TotalSeconds - _stateProvider.AzimuthWasteTime));
            degreeDriven = driven.Direction == DriveDirection.AzimuthPositive ? degreeDriven : degreeDriven * -1;
            var newAngle = source.Azimuth + degreeDriven;
            newAngle = Math.Clamp(newAngle, _deviceSettings.MinAzimuth, _deviceSettings.MaxAzimuth);

            var result = new Orientation(newAngle, source.Altitude, target.ValidUntil);
            return result;
        }

        private async Task<Orientation> DriveAltitude(Orientation source, Orientation target, CancellationToken token)
        {
            //turn differences in direction and time altitude 
            var direction = target.Altitude > source.Altitude
                ? DriveDirection.AltitudePositive
                : DriveDirection.AltitudeNegative;
            //already in limit
            if (CheckLimit(direction))
            {
                return source with { ValidUntil = target.ValidUntil };
            }
            var driveAngle = Math.Abs(target.Altitude - source.Altitude);
            if (driveAngle < _appSettings.AltitudeMinAngleForDrive)
            {
                return source with { ValidUntil = target.ValidUntil };
            }


            var time = driveAngle / _stateProvider.AltitudeDegreePerSecond;
            var timeWithWaste = time + _stateProvider.AltitudeWasteTime;
            var driven = await _ioService.Drive(
                direction,
                TimeSpan.FromSeconds(timeWithWaste),
                token);


            var degreeDriven = Convert.ToSingle(_stateProvider.AltitudeDegreePerSecond * (driven.TimeDriven.TotalSeconds - _stateProvider.AltitudeWasteTime));
            degreeDriven = driven.Direction == DriveDirection.AltitudePositive ? degreeDriven : degreeDriven * -1;
            var newAngle = source.Altitude + degreeDriven;
            newAngle = Math.Clamp(newAngle, _deviceSettings.MinAltitude, _deviceSettings.MaxAltitude);


            var result = new Orientation(source.Azimuth, newAngle, target.ValidUntil);
            return result;
        }

        private bool CheckLimit(DriveDirection direction)
        {
            var result = direction switch
            {
                DriveDirection.AzimuthNegative => _ioService.AzimuthMinLimit,
                DriveDirection.AzimuthPositive => _ioService.AzimuthMaxLimit,
                DriveDirection.AltitudeNegative => _ioService.AltitudeMinLimit,
                DriveDirection.AltitudePositive => _ioService.AltitudeMaxLimit,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
            return result;
        }

    }
}
