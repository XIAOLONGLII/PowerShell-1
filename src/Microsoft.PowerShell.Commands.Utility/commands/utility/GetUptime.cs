/********************************************************************++
Copyright (c) Microsoft Corporation.  All rights reserved.
--********************************************************************/

using System;
using System.Management.Automation;
using System.Diagnostics;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// This class implements Get-Uptime
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Uptime", DefaultParameterSetName = TimespanParameterSet, HelpUri = "")]
    [OutputType(typeof(TimeSpan), ParameterSetName = new string[] { TimespanParameterSet })]
    [OutputType(typeof(DateTime), ParameterSetName = new string[] { SinceParameterSet })]
    public class GetUptimeCommand : PSCmdlet
    {
        /// <summary>
        /// Since parameter
        /// The system startup time
        /// </summary>
        /// <value></value>
        [Parameter(ParameterSetName = SinceParameterSet)]
        public SwitchParameter Since { get; set; } = new SwitchParameter();

        /// <summary>
        /// ProcessRecord() override
        /// This is the main entry point for the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            // Get-Uptime throw if IsHighResolution = false  
            // because stopwatch.GetTimestamp() return DateTime.UtcNow.Ticks
            // instead of ticks from system startup.
            // InternalTestHooks.StopwatchIsNotHighResolution is used as test hook.
            if (Stopwatch.IsHighResolution && !InternalTestHooks.StopwatchIsNotHighResolution)
            {
                TimeSpan uptime = TimeSpan.FromSeconds(Stopwatch.GetTimestamp()/Stopwatch.Frequency); 

                switch (ParameterSetName)
                {
                    case TimespanParameterSet:
                        // return TimeSpan of time since the system started up
                        WriteObject(uptime);
                        break;
                    case SinceParameterSet:
                        // return Datetime when the system started up
                        WriteObject(DateTime.Now.Subtract(uptime));
                        break;
                }
            }
            else
            {
                WriteDebug("System.Diagnostics.Stopwatch.IsHighResolution returns 'False'.");
                Exception exc = new NotSupportedException(GetUptimeStrings.GetUptimePlatformIsNotSupported);
                ThrowTerminatingError(new ErrorRecord(exc, "GetUptimePlatformIsNotSupported", ErrorCategory.NotImplemented, null));
            }
        }

        /// <summary>
        /// Parameter set name for Timespan OutputType.
        /// </summary>
        private const string TimespanParameterSet = "Timespan";

        /// <summary>
        /// Parameter set name for DateTime OutputType.
        /// </summary>
        private const string SinceParameterSet = "Since";
   }
}
