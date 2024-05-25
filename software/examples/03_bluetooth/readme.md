# Bluetooth 01 Project

I'm trying to get bluetooth working between my Computer and my ESP32 microcontroller.

I'm following [this tutorial](https://github.com/finallyfunctional/bluetooth-windows-esp32-example), and I had to make the following changes to get it building and running on my machine:

### Computer Side of Things

## Download C++ Tools for Visual Studio 2022

When I first loaded the project, I needed to install the C++ development tools. That was easy.

## Wasn't Linking Ws2_32.lib

Then I built the project, and encountered the following errors:

```
Build started...
1>------ Build started: Project: WindowsBtWithEsp32, Configuration: Debug x64 ------
1>WindowsBtWithEsp32.cpp
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_closesocket referenced in function "bool __cdecl sendMessageToEsp32(void)" (?sendMessageToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_connect referenced in function "bool __cdecl connectToEsp32(void)" (?connectToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_ioctlsocket referenced in function "bool __cdecl connectToEsp32(void)" (?connectToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_recv referenced in function "bool __cdecl recieveMessageFromEsp32(void)" (?recieveMessageFromEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_send referenced in function "bool __cdecl sendMessageToEsp32(void)" (?sendMessageToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_socket referenced in function "bool __cdecl connectToEsp32(void)" (?connectToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_WSAStartup referenced in function "bool __cdecl startupWindowsSocket(void)" (?startupWindowsSocket@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_WSACleanup referenced in function "bool __cdecl sendMessageToEsp32(void)" (?sendMessageToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol __imp_WSAGetLastError referenced in function "bool __cdecl connectToEsp32(void)" (?connectToEsp32@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol BluetoothFindFirstDevice referenced in function "bool __cdecl getPairedEsp32BtAddress(void)" (?getPairedEsp32BtAddress@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol BluetoothFindNextDevice referenced in function "bool __cdecl getPairedEsp32BtAddress(void)" (?getPairedEsp32BtAddress@@YA_NXZ)
1>C:\dev\bluetooth-windows-esp32-example\WindowsBtWithEsp32\x64\Debug\WindowsBtWithEsp32.exe : fatal error LNK1120: 11 unresolved externals
1>C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\VC\v170\Microsoft.CppCommon.targets(1127,5): error MSB6006: "link.exe" exited with code 1120.
1>Done building project "WindowsBtWithEsp32.vcxproj" -- FAILED.
========== Build: 0 succeeded, 1 failed, 0 up-to-date, 0 skipped ==========
========== Build started at 8:07 PM and took 03.596 seconds ==========
```

To fix this, I added `Ws2_32.lib` into my Additional Dependencies. As described in this [stack overflow](https://stackoverflow.com/a/53873194)

* Right click on my "WindowsBtWithEsp32" project in Solution Explorer
* Click Properties
* Go to Linker => Input. Click to edit Additional Dependencies
* Add `Ws2_32.lib` in the first box:
![Adding dependency](image-1.png)

## Didn't Have Pragma comment

Then I built again, and received the following error:

```
Build started...
1>------ Build started: Project: WindowsBtWithEsp32, Configuration: Debug x64 ------
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol BluetoothFindFirstDevice referenced in function "bool __cdecl getPairedEsp32BtAddress(void)" (?getPairedEsp32BtAddress@@YA_NXZ)
1>WindowsBtWithEsp32.obj : error LNK2019: unresolved external symbol BluetoothFindNextDevice referenced in function "bool __cdecl getPairedEsp32BtAddress(void)" (?getPairedEsp32BtAddress@@YA_NXZ)
1>C:\dev\bluetooth-windows-esp32-example\WindowsBtWithEsp32\x64\Debug\WindowsBtWithEsp32.exe : fatal error LNK1120: 2 unresolved externals
1>Done building project "WindowsBtWithEsp32.vcxproj" -- FAILED.
========== Build: 0 succeeded, 1 failed, 0 up-to-date, 0 skipped ==========
========== Build started at 8:12 PM and took 00.453 seconds ==========
```

To fix this I added this comment (`#pragma comment(lib, "Bthprops.lib")`) below my include statements

![programming includes](image.png)

Then I could build and run the solution:

![sample output](image-2.png)

### ESP Side of Things

I didn't realize I the Esp32-S3 only uses Bluetooth 5, not bluetooth classic. The esp documentation is confusing.

I checked my windows Bluetooth version by:

* Going to device manager
* Opening bluetooth
* Going to advanced
* Referencing the underscored version:

![alt text](image-4.png)

* Comparing it against the list located here:

![alt text](image-5.png)
[bluetooth versions](https://support.microsoft.com/en-us/windows/what-bluetooth-version-is-on-my-pc-f5d4cff7-c00d-337b-a642-d2d23b082793)


### Running Code on ESP

Found some documentation:

https://github.com/nkolban/ESP32_BLE_Arduino/blob/master/examples/BLE_write/BLE_write.ino

https://github.com/microsoft/Windows-universal-samples/tree/main

https://github.com/sensboston/BLEConsole

https://randomnerdtutorials.com/esp32-ble-server-client/

https://github.com/pcbreflux/espressif/blob/master/esp32/app/ESP32_ble_i2c_ina219/main/GreatNeilKolbanLib/BLEAdvertising.h

### Debugging ESP Side of Things

When using the BLEConsole to attempt to connect to ESP, I get the following errors:

![alt text](image-6.png)

## Forked Repo with Fixes In It

I'm thankful to [finally functional](https://github.com/finallyfunctional) for code on how to get started. I've forked his repo and included the 2 fixes I mentioned above in my fork. Hopefully this gives people enough info to avoid these same issues locally.

* Original Repo: https://github.com/finallyfunctional/bluetooth-windows-esp32-example
* Forked Repo: https://github.com/morgankenyon/bluetooth-windows-esp32-example


## Running

Server: 

![alt text](image-9.png)

Client: 

![alt text](image-8.png)