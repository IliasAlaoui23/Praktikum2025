$taskName = "CashRecyclerJob"

$taskDescription = "Runs the C# application every 10 minutes." 

$applicationPath = "C:\Users\ialaouik\RiderProjects\ConsoleApp2\ConsoleApp2\ConsoleApp2.csproj" 

$trigger = New-ScheduledTaskTrigger -AtStartup -Repeat -Interval (New-TimeSpan -Minutes 10) -RepetitionDuration ([TimeSpan]::MaxValue) 

$action = New-ScheduledTaskAction -Execute $applicationPath

$scheduledTask = New-ScheduledTask -Action $action -Trigger $trigger -Description $taskDescription -User "SYSTEM" -RunLevel Highest

Register-ScheduledTask -TaskName $taskName -InputObject $scheduledTask 

Write-Host "Scheduled task '$taskName' created successfully to run every 10 minutes."
