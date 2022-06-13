using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NasaApi.Models
{
    public class NEOInfo
    {
        public string Id { get; set; }

        public string NeoReferenceId { get; set; }

        public string Name { get; set; }

        public string NasaJPLUrl { get; set; }

        public string EstimatedDiameterInMetersMin { get; set; }

        public string EstimatedDiameterInMetersMax { get; set; }

        public string IsPotentiallyHazardousAsteroid { get; set; }

        public string CloseApproachDate { get; set; }

        public string MilesPerHour { get; set; }

        public string MissDistanceInMiles { get; set; }

        public string OrbitingBody { get; set; }

        public string IsSentryObject { get; set; }
    }
}
