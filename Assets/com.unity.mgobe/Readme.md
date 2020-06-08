# 目标
- 该DEMO的功能和GameDeck/GSE的DEMO对齐，实现基于Unity Learn Micro Karting项目的多人卡丁车对战游戏
- 未来在两个DEMO的基础上抽取公共接口，使开发者可以切换框架实现
- 该DEMO在2020年2月完成
- 12月上旬@Z 完成技术调研，确保技术可行性
- 贾聪配合@Z 一起完成整个DEMO

# 功能
- [P0] 对接房间API，实现创建删除，进入退出，列出房间等功能
- [P0] 实现在房间中等待所有玩家READY，所有玩家READY后可以开始游戏
- [P0] 对接帧同步服务器，实现多人联机卡丁车
- [P0] 将需要用到的小游戏引擎的API，实现C#版本的SDK
- [P1] 对接小游戏的match making功能
- [P2] 对接小程序登陆功能，实现用微信扫码登陆游戏
- [P3] 对接小游戏数据存储功能，退出游戏时记录单圈最快时间
- [P3] 客户端实现差值帧补偿，让游戏体验更加顺畅

# 更新 PB 文件
protoc --proto_path=pb --csharp_out=pb --csharp_opt=base_namespace=Lagame proto.proto