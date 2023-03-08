namespace SolarTracker.Services
{
    public class DriveService
    {
        private readonly IIoService _ioService;
        public DriveService(IIoService ioService)
        {
            _ioService = ioService;
        }


        public Task<Orientation> DoStartupProcedure(CancellationToken token)
        {
            //drive down
            //drive left

            //drive up in 1 go (time for angle)
            //drive back in n go´s (time for start/stop)

            //drive right in 1 go (time for angle)
            //drive back in n go´s (time for start/stop)

            //set current position.

            //here we should know how much is the drive integration delay,
            //and how much angle do we cover per time.
            //Therefor we can calculate later how much time to drive....

            return null!;
        }


        public Task<Orientation> DriveToTarget(Orientation source, Orientation target, CancellationToken token)
        {
            //check difference for target and current position
            //maybe no need to move

            //turn differences in direction and time azimuth
            //turn differences in direction and time altitude 

            //maybe move azimuth + update current Position

            //maybe move altitude + update current Position


            //special rules:
            //-when we are over or close to limits, we drive up to the limit
            //-we are saving drive results over the day


            return null!;
        }



    }
}
