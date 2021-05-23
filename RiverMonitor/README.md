# RiverMonitor

Retrieves river measurements and triggers lighting shows.

## Webhook

A webhook needs to be set up on the Particle console: [https://console.particle.io/](https://console.particle.io/)

### Webhook Settings:

 - Event Name: mississippi-stpaul-discharge
 - URL: https://waterservices.usgs.gov/nwis/iv/
 - Request Type: GET
 - Request Format: Query Parameters
 - Device: Any
 - Advanced Settings:
   - Query Parameters:
     - format = json
     - sites = 05331000
     - parameterCd = 00060
     - siteStatus = all

## Events

Particle events are messages sent to the Particle cloud platform from the device, or from the cloud platform to the device. They are used to coordinate actions that should be taken or record information about what's happening.

 - minneapolis-505FourthAveS-test
    message from the cloud platform requesting that the device toggle testing mode
 - minneapolis-505FourthAveS-discharge-measurement-trigger
    message from the cloud platform requesting that the device initiate a measurement retrieval
 - mississippi-stpaul-discharge multi-part 
    message for the device containing a USGS Water Services discharge measurement, also used by the device to request this data
 - minneapolis-505FourthAveS-discharge-measurement-retrieved 
    message from the device indicating that a discharge measurement has been successfully retrieved
 - minneapolis-505FourthAveS-set-lighting-program
    message from the device indicating that the lighting program has been changed

## Particle Platform Information

Every new Particle project is composed of 3 important elements that you'll see have been created in your project directory for RiverMonitor.

#### ```/src``` folder:  
This is the source folder that contains the firmware files for your project. It should *not* be renamed. 
Anything that is in this folder when you compile your project will be sent to our compile service and compiled into a firmware binary for the Particle device that you have targeted.

If your application contains multiple files, they should all be included in the `src` folder. If your firmware depends on Particle libraries, those dependencies are specified in the `project.properties` file referenced below.

#### ```.ino``` file:
This file is the firmware that will run as the primary application on your Particle device. It contains a `setup()` and `loop()` function, and can be written in Wiring or C/C++. For more information about using the Particle firmware API to create firmware for your Particle device, refer to the [Firmware Reference](https://docs.particle.io/reference/firmware/) section of the Particle documentation.

#### ```project.properties``` file:  
This is the file that specifies the name and version number of the libraries that your project depends on. Dependencies are added automatically to your `project.properties` file when you add a library to a project using the `particle library add` command in the CLI or add a library in the Desktop IDE.

## Adding additional files to your project

#### Projects with multiple sources
If you would like add additional files to your application, they should be added to the `/src` folder. All files in the `/src` folder will be sent to the Particle Cloud to produce a compiled binary.

#### Projects with external libraries
If your project includes a library that has not been registered in the Particle libraries system, you should create a new folder named `/lib/<libraryname>/src` under `/<project dir>` and add the `.h`, `.cpp` & `library.properties` files for your library there. Read the [Firmware Libraries guide](https://docs.particle.io/guide/tools-and-features/libraries/) for more details on how to develop libraries. Note that all contents of the `/lib` folder and subfolders will also be sent to the Cloud for compilation.

## Compiling your project

When you're ready to compile your project, make sure you have the correct Particle device target selected and run `particle compile <platform>` in the CLI or click the Compile button in the Desktop IDE. The following files in your project folder will be sent to the compile service:

- Everything in the `/src` folder, including your `.ino` application file
- The `project.properties` file for your project
- Any libraries stored under `lib/<libraryname>/src`
