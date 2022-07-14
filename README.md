# DeviceService

*设备集成类库*

**Net6框架版本**

## 项目层级

### DeviceModel
设备的基类，通常表示某种类型（品牌）的设备，有时也会表示具体的设备（为具体产品的基类）

### lib
设备的第三方sdk动态类库（dll）文件夹

### Model
类库的model类

>#### ExceptionModels
>设备的异常类，用于显示设备发生的异常信息

### Product
产品类,继承与DeviceModel中的具体业务类，将DeviceModel中的类进行迭代扩展以实现具体业务

### SDK
为第三方厂家提供的SDK类，部分DeviceModel是基于SDK进行的二次开发

### Device
接口类，抽象化的行为，通常由Product偶尔使用DeviceModel来进行具体的实现

***

## 当前集成的产品

* ### Arcface(虹软人脸识别算法)
* ### CardWriteCETC(CETC(黑盒子)型号发卡器)
* ### GReader(GReader型号读写器)
* ### HIKVISION(海康威视设备)
> CameraHIK(海康摄像头)
> EntranceGuardHIK(海康门禁)
* ### Protocol(以通讯协议对接的设备)
> CabinetSilverBox(银箱智能柜锁控板)
* ### RSNet(仁硕电子网络设备)
> CentralControlRS(仁硕中控机)
> TempHumMeterRS(仁硕温湿度)
* ### UHFGate(UHFGate型号读写器)
> ChannelGateUHFGate(UHFGate通道门)                                                                                                                                                                                                                                                                                                                
* ### UHFReader09(UHFReader09型号读写器)
* ### UHFReader288(UHFReader288型号读写器)
> ChannelGateUHF288(UHF288通道门)
* ### UHFReader86(UHFReader86型号读写器)
* ### UHFReaderCETC(中电海康读写器)
* ### UHFReaderCETCServer(中电海康读写器服务端模式)
* ### UHFReaderRFID(UHFRFID读写器设备封装类库)
* ### ZKTECO(熵基科技产品)
> Live20FS(熵基中控指纹采集仪)
* ### ICReader(IC读卡器)
