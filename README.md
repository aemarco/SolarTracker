# SolarTracker

This project is running in my garden on a raspberry pi 2 zero W since a while.
Reaches the appox. 40% increase in production, and works stable as a rock^^


## Overview

This app has been built to control the orientation of a solar panel in order to maximize it´s production.
The key point here is, that no expensive servo drives are used. Instead cheaper inear drives do the job.
During daylight hours it follows the sun in a given interval, and in the evening returns to the morning position, which it keeps during the night.

- Can run on a general purpose device with GPIO pins (e.g. raspberry pi 2 zero W in my case)
- Behaviour stuff can be adjusted in appsettings.json
- Device related stuff such as Pin´s used angle´s etc. can be adjusted in devicesettings.json
- [Geolocation-Api](https://ipgeolocation.io/) is used to get target data for adjusted location 
- The web api might be explored through Swagger which can be enabled. It offers some insights into the app and it´s state and also some manual controls for driving.
- There´s some fallback logic to bridge internet outages


## Setup

Azimuth and Altitude each requires to have limit switches at known angle´s. 
Those angles and the pinout to be adjusted in devicesettings along with also the pins for driving in those directions.

- Example:
"MinAzimuth" would be the minimum angle sideways to the left. 
"MinAzimuthLimitPin" would be the pin for the limit switch for this position
"AzimuthDriveNegativePin" would be the pin to drive towards this position (to the left)

IpGeolocationClientSettings requires an Api key for gathering target data. Can be optained there, and the free plan should be sufficient for this usage.

Adjust however logs shall be handled via the Serilog section in appsettings.json

Disable "Auto" and enable "EnableSwaggerUi", so you can check if all the hardware setup works as suppsed to.

"deployToPi.ps1" can deploy the thing to a Pi... but ssh and it´s auth needs some care before

Once all the hardware works as expected, enable "Auto" and restart!


## Caveats / Pitfalls

- Don´t start the thing, before the system clock isn´t up to date (raspberry forget´s it´s time on power loss)... see "ExecStartPre" in deployToPi.ps1
- "ShutdownAfterSunset" does what it says, but you then somehow need to power cycle it before sunrise to restart.
- Works only in northern hemisphere. (Code would need some work regarding directions to support the southern regions)
- Running user on the Pi requires to have the rights to read/write GPIO pins
