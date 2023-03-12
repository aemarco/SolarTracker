using System.Collections.Generic;
using System.Linq;

namespace SolarTracker.Services
{
    public class DriveService
    {
        private readonly DeviceSettings _deviceSettings;
        private readonly AppSettings _appSettings;
        private readonly IIoService _ioService;
        public DriveService(
            DeviceSettings deviceSettings,
            AppSettings appSettings,
            IIoService ioService)
        {
            _deviceSettings = deviceSettings;
            _appSettings = appSettings;
            _ioService = ioService;
        }


        public Task<Orientation> DoStartupProcedure(CancellationToken token)
        {
            //drive down
            _ = _ioService.Drive(DriveDirection.AltitudeNegative, TimeSpan.MaxValue, token);
            //drive left
            _ = _ioService.Drive(DriveDirection.AzimuthNegative, TimeSpan.MaxValue, token);



            //drive up in 1 go (time for angle)
            var up = _ioService.Drive(DriveDirection.AltitudePositive, TimeSpan.MaxValue, token);
            if (!_ioService.AltitudeMaxLimit)
                throw new Exception("could not reach max altitude");
            //drive back in n go´s (time for start/stop)
            var down = new List<DriveResult>();
            while (!_ioService.AltitudeMinLimit && down.Count < 20)
            {
                var timeToDrive = up.TimeDriven.TotalSeconds / 10;
                down.Add(_ioService.Drive(DriveDirection.AltitudeNegative, TimeSpan.FromSeconds(timeToDrive), token));
            }
            if (!_ioService.AltitudeMinLimit)
                throw new Exception("could not reach min altitude");
            var altWasted = down.Sum(x => x.TimeDriven.TotalSeconds) - up.TimeDriven.TotalSeconds;
            _altWasteTime = Convert.ToSingle(altWasted / down.Count);
            _altDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAltitude - _deviceSettings.MinAltitude)
                                  / up.TimeDriven.TotalSeconds);



            //drive right in 1 go (time for angle)
            var right = _ioService.Drive(DriveDirection.AzimuthPositive, TimeSpan.MaxValue, token);
            if (!_ioService.AzimuthMaxLimit)
                throw new Exception("could not reach max azimuth");
            //drive back in n go´s (time for start/stop)
            var left = new List<DriveResult>();
            while (!_ioService.AzimuthMinLimit && left.Count < 20)
            {
                var timeToDrive = right.TimeDriven.TotalSeconds / 10;
                left.Add(_ioService.Drive(DriveDirection.AzimuthNegative, TimeSpan.FromSeconds(timeToDrive), token));
            }
            if (!_ioService.AzimuthMinLimit)
                throw new Exception("could not reach min azimuth");
            var aziWasted = left.Sum(x => x.TimeDriven.TotalSeconds) - right.TimeDriven.TotalSeconds;
            _aziWasteTime = Convert.ToSingle(aziWasted / left.Count);
            _aziDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAzimuth - _deviceSettings.MinAzimuth)
                                  / right.TimeDriven.TotalSeconds);

            //here we should know how much is the drive integration delay,
            //and how much angle do we cover per time.
            //Therefor we can calculate later how much time to drive....


            //set current position.
            return Task.FromResult(
                new Orientation(
                    _deviceSettings.MinAzimuth,
                    _deviceSettings.MinAltitude,
                    DateTime.Now.Add(_appSettings.AutoInterval)));
        }




        public async Task<Orientation> DriveToTarget(Orientation? source, Orientation target, CancellationToken token)
        {
            source ??= await DoStartupProcedure(token);

            var result = DriveAzimuth(source, target, token);
            result = DriveAltitude(result, target, token);
            return result;

            //special rules:
            //-when we are over or close to limits, we drive up to the limit
            //-we are saving drive results over the day
        }


        private float _aziDegreePerSecond;
        private float _aziWasteTime;
        private Orientation DriveAzimuth(Orientation source, Orientation target, CancellationToken token)
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


            var time = Math.Abs(target.Azimuth - source.Azimuth) / _aziDegreePerSecond;
            var timeWithWaste = time + _aziWasteTime;
            var driven = _ioService.Drive(
                direction,
                TimeSpan.FromSeconds(timeWithWaste),
                token);


            var degreeDriven = Convert.ToSingle(_aziDegreePerSecond * (driven.TimeDriven.TotalSeconds - _aziWasteTime));
            degreeDriven = driven.Direction == DriveDirection.AzimuthPositive ? degreeDriven : degreeDriven * -1;
            var newAngle = source.Azimuth + degreeDriven;
            newAngle = Math.Clamp(newAngle, _deviceSettings.MinAzimuth, _deviceSettings.MaxAzimuth);

            var result = new Orientation(newAngle, source.Altitude, target.ValidUntil);
            return result;
        }

        private float _altDegreePerSecond;
        private float _altWasteTime;
        private Orientation DriveAltitude(Orientation source, Orientation target, CancellationToken token)
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


            var time = Math.Abs(target.Altitude - source.Altitude) / _altDegreePerSecond;
            var timeWithWaste = time + _altWasteTime;
            var driven = _ioService.Drive(
                direction,
                TimeSpan.FromSeconds(timeWithWaste),
                token);


            var degreeDriven = Convert.ToSingle(_altDegreePerSecond * (driven.TimeDriven.TotalSeconds - _altWasteTime));
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
