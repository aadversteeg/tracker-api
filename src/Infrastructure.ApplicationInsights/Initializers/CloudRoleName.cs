using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Infrastructure.ApplicationInsights.Initializers
{
    public class CloudRoleName : ITelemetryInitializer
    {
        private readonly string roleName;

        public CloudRoleName(string roleName)
        {
            this.roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = this.roleName;
        }
    }
}
