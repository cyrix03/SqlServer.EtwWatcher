# SqlServer.EtwWatcher

This project is a worker service application built on .net core 3.0. The service will listen to Event Tracing for Windows (ETW) events and publish them to a configurable Log Analytics Azure Resource. Please note this is a work in progress and is provided as is.

## Getting Started

To get the project running successfully you will need a Sql Server instance running with Extended Events turned on, and an instance of the Log Analytics Azure Resource with a custom log enabled.

### Prerequisites

The Event Session name used on creation will be needed later for configuration. Below is how to enable Deadlock Events with the default session name. At this time, deadlocks are the only events that are handled.

```
CREATE EVENT SESSION [XE_DEFAULT_ETW_SESSION]
    ON SERVER 
	ADD EVENT sqlserver.blocked_process_report(
        ACTION (
			sqlserver.client_app_name,
			sqlserver.client_hostname,
			sqlserver.database_name)),
	ADD EVENT sqlserver.xml_deadlock_report (
		ACTION (
			sqlserver.client_app_name,
			sqlserver.client_hostname,
			sqlserver.database_name))
	ADD TARGET package0.etw_classic_sync_target
	WITH (
		MAX_MEMORY=4096 KB,
		EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,
		MAX_DISPATCH_LATENCY=30 SECONDS,
		MAX_EVENT_SIZE=0 KB,
		MEMORY_PARTITION_MODE=NONE,
		TRACK_CAUSALITY=OFF,
		STARTUP_STATE=ON )
GO
```

### Installing

The app should run right out of the box assuming one provides valid configuration in `app.config.json`. 

```
{
    "EtwWatcherAzureConfiguration": {
        "LogAnalyticsSharedKey": "{SHARED KEY GOES HERE}",
        "LogAnalyticsCustomerId": "{AZURE RESOURCE GUID GOES HERE}"
    },
    "EtwWatcherInstanceConfiguration": {
        "ConnectionString": "Data Source=(local);Integrated Security=true",
        "SessionName": "XE_DEFAULT_ETW_SESSION"
    }
}

```

## Running the tests

Unit tests are created using [NUnit](https://nunit.org/). They validate message types are created correctly and build signatures generate the correct hash to send Azure.

## Deployment

A 'how-to' guide to get this service installed on a remote machine needs to be added.

## Built With

* [Autofac](https://github.com/autofac/Autofac) - Dependency Injection Container
* [Serilog](https://github.com/serilog/serilog) - Diagnotic Logging Library
* [ASP.NET Core Background Tasks](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.0&tabs=visual-studio) - Worker Service Template
* [Micosoft.SqlServer.XEvent.XELite](https://www.nuget.org/packages/Microsoft.SqlServer.XEvent.XELite/) - Microsoft seemed to have removed all their documentation for this package. This provides a way to consume SQL Server Extended Events.

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Authors

* **Andrew Warren** - *Initial work* - (https://github.com/cyrix03)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Custom logs in Log Analytics - https://docs.microsoft.com/en-us/azure/azure-monitor/platform/data-collector-api
* SQL Server Monitoring Solutions are Expensive. Build your own tools.
