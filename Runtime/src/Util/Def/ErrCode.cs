namespace com.unity.mgobe
{
    public class SdkErrMsgEnum
    {
        public const string EcSdkSendFail  = "发送失败";
        public const string EcSdkUninit = "未初始化";
        public const string EcSdkResTimeout = "发送超时";
        public const string EcSdkNoLogin = "未登录";
        public const string EcSdkNoCheckLogin = "CheckLogin失败";
        public const string EcSdkSocketError = "Socket错误";
        public const string EcSdkSocketClose = "Socket断开";
        public const string EcSdkNoRoom = "无房间信息";
    }

    public class ErrCode
    {

        //系统框架错误
        public const int EcOk = 0;  // 返回成功
        public const int EcReqBadPkg = 1;  // 请求包格式错误
        public const int EcCmdInvalid = 2;  // 非法命令字
        public const int EcParamsInvalid = 3;  // 参数错误
        public const int EcInnerError = 4;  // 服务器内部错误
        public const int EcTimeOut = 5;  // 后端超时错误
        public const int EcServerBusy = 6;  // 服务器繁忙
        public const int EcNoRight = 7;  // 没有权限请求

        //接入层错误码
        public const int EcAccessCmdInvalidErr = 200;  // 命令字无效错误
        public const int EcAccessCmdGetTokenErr = 201;  // 获取Token失败
        public const int EcAccessCmdTokenPreExpire = 202;  // Token即将过期
        public const int EcAccessCmdInvalidToken = 203;  // Token无效或过期
        public const int EcAccessPushSerializeErr = 204;  // PUSH序列化包失败
        public const int EcAccessLoginBodyParseErr = 205;  // 登陆用户中心回包解析出错
        public const int EcAccessConnErr = 206;  // 查找连接信息出错
        public const int EcAccessGetRsIpErr = 207;  // 获取Relay的RS_IP或RS_PORT出错
        public const int EcAccessAddCommConnErr = 208;  // 添加COMM连接信息失败
        public const int EcAccessAddHeartConnErr = 209;  // 添加心跳连接信息失败
        public const int EcAccessAddRelayConnErr = 210;  // 添加Relay连接信息失败
        public const int EcAccessHeartBodyParseErr = 211;  // 心跳包解析出错
        public const int EcAccessGetCommConnectErr = 212;  // 获取COMM连接信息失效
        public const int EcAccessGetRelayConnectErr = 213;  // 获取RELAY连接信息失效
        public const int EcAccessAccessInfoEmpty = 214;  // 连接信息为空
        public const int EcAccessPlayerDuplicateLogin = 215;  // 用户已经登录，不能重复登录

        //用户中心错误（10000～19999）
        public const int EcPlayerGameNotExist = 10000;   // game不存在
        public const int EcPlayerSecretKeyFail = 10001;   // 查询secret_key失败
        public const int EcPlayerSignErr = 10002;   // sign校验失败
        public const int EcPlayerDuplicateReq = 10003;   // 重复请求
        public const int EcPlayerTimestampInvalid = 10004;   // timestamp非法
        public const int EcPlayerQueryPlayerFail = 10005;   // 查询用户信息失败
        public const int EcPlayerAddPlayerFail = 10006;   // 新增用户信息失败
        public const int EcPlayerQueryGameFail = 10007;   // 查询game信息失败
        public const int EcPlayerRecordNumErr = 10008;   // 用户记录数不正确
        public const int EcPlayerGetTokenFail = 10009;   // 查询token失败
        public const int EcPlayerTokenNotExist = 10010;   // token不存在
        public const int EcPlayerTokenInvalid = 10011;   // token非法
        public const int EcPlayerClearTokenFail = 10012;   // 清除token缓存失败
        public const int EcPlayerLockFail = 10013;   // 获取分布式锁失败
        public const int EcPlayerUnlockFail = 10014;   // 释放分布式锁失败
        public const int EcPlayerSaveTokenFail = 10015;   // 保存token缓存失败

        //房间管理类错误（20000-29999）
        public const int EcRoomCreateNoPermission = 20000; // 创建房间无权限
        public const int EcRoomDestoryNoPermission = 20001; // 销毁房间无权限
        public const int EcRoomJoinNoPermission = 20002; // 无权限加入房间
        public const int EcRoomRemovePlayerNoPermission = 20003; // 无踢人权限
        public const int EcRoomModifyPropertiesNoPemission = 20004; // 无修改房间属性权限
        public const int EcRoomDissmissNoPermission = 20005; // 无解散房间权限
        public const int EcRoomRemoveSelfNoPermission = 20006; // 无踢出自己权限
        public const int EcRoomCheckLoginSessionErr = 20007; // 检查登录失败
        public const int EcRoomPlayerAlreadyInRoom = 20010; // 用户已经在房间内，不能操作创建房间、加房等操作
        public const int EcRoomPlayerNotInRoom = 20011; // 用户目前不在房间内，不能操作更改房间属性、踢人等操作
        public const int EcRoomPlayersExceedLimit = 20012; // 房间内用户数已经达到最大人数不能再加入了
        public const int EcRoomJoinNotAllow = 20013; // 房间不允许加入用户
        public const int EcRoomMaxPlayersInvalid = 20014; // 最大用户数值设置非法
        public const int EcRoomCreateFail = 20015; // 创建房间失败
        public const int EcRoomPlayerOffline = 20016; // 用户在房间中掉线，不能开始游戏等操作
        public const int EcRoomParamPageInvalid = 20017; // 页号、页数大小参数不合法，可能实际大小没这么大
        public const int EcRoomGetPlayerInfoErr = 20050; // 查询用户信息失败
        public const int EcRoomGetRoomInfoErr = 20051; // 获取房间信息失败
        public const int EcRoomModifyOwnerErr = 20052; // 修改房主失败
        public const int EcRoomMaxRoomNumberExceedLimit = 20053; // 房间数量超过限制
        public const int EcRoomRemoveRedisPlayerRoomMatchErr = -20052; // 删除用户房间映射表信息失败
        public const int EcRoomRemoveRedisRoomInfoErr = -20053; // 删除房间信息表信息失败
        public const int EcRoomRedisUpdateErr = -20054; // 数据库更新失败
        public const int EcRoomRedisGetLockErr = -20055; // 获取锁失败
        public const int EcRoomRedisCheckLockErr = -20056; // 检查锁失败，一般是过期
        public const int EcRoomRedisDelLockErr = -20057; // 删除锁失败
        public const int EcRoomQueryPlayerErr = 20060; // 查询用户信息失败
        public const int EcRoomQueryGameErr = 20061; // 游戏信息失败
        public const int EcRoomPlayerInfoNotExist = 20062; // 用户信息不存在
        public const int EcRoomGameInfoNotExist = 20063; // 游戏信息不存在
        public const int EcRoomHistoryInfoInsertErr = -20064; // mysql数据库插入历史房间信息失败
        public const int EcRoomRegionInfoNotExist = 20065; // 查询不到accessRegion信息
        public const int EcRoomQueryRegionErr = 20066; // 查询地域信息失败
        public const int EcRoomInfoUnexist = 20080; // 房间信息不存在
        public const int EcRoomAllocateRelaysvrIpPortErr = 20090; // ctrlsvr分配relaysvr失败
        //teams
        public const int EcRoomInvalidParamsTeamId = 20100; // 房间teamId无效
        public const int EcRoomTeamMemberLimitExceed = 20101; // 房间团队人员已满

        //匹配服务类错误 （30000～39999）
        public const int EcMatchNoRoom = 30000; // 匹配失败，无任何房间
        public const int EcMatchTimeout = 30001; // 匹配超时
        public const int EcMatchLogicErr = 30002; // 匹配逻辑错误
        public const int EcMatchErr = 30010; // 匹配失败
        public const int EcMatchPlayerIsInMatch = 30011; // 用户已经在匹配中
        public const int EcMatchPlayerNotInMatch = 30012; // 用户不在匹配状态
        public const int EcMatchGetMatchInfoErr = 30013; // 获取匹配信息失败
        public const int EcMatchUpdateMatchInfoErr = 30014; // 更新匹配信息失败
        public const int EcMatchCancelFailed = 30015; // 取消匹配失败
        public const int EcMatchGetPlayerListInfoErr = 30016; // 查询匹配队列信息失败
        public const int EcMatchCanNotFound = 30040; // [rm] 当前大区找不到合适的匹配,内部接口用
        public const int EcMatchCreateRoomErr = 30041; // 匹配创建房间失败
        public const int EcMatchJoinRoomErr = 30042; // 匹配加入房间失败
        public const int EcMatchQueryPlayerErr = 30100; // 查询用户信息失败
        public const int EcMatchPlayerInfoNotExist = 30101; // 用户信息不存在
        public const int EcMatchQueryGameErr = 30102; // 查询游戏信息失败
        public const int EcMatchGameInfoNotExist = 30103; // 游戏信息不存在
        public const int EcMatchQueryRegionErr = 30104; // 查询大区信息失败
        public const int EcMatchRegionInfoNotExist = 30105; // 无大区信息
        public const int EcMatchTeamFail = 30106; // 团队匹配失败
        public const int EcMatchPlayRuleNotRunning = 30107; // 匹配规则不可用
        public const int EcMatchPlayAttrNotFound = 30108; // 匹配参数不完整
        public const int EcMatchPlayRuleNotFound = 30109; // 匹配规则不存在
        public const int EcMatchPlayRuleAttrSegmentNotFound = 30110; // 匹配规则获取属性匹配区间失败
        public const int EcMatchPlayRuleFuncErr = 30111; // 匹配规则算法错误
        public const int EcMatchGetPlayerAttrFail = 30112; // 匹配获取玩家属性失败
        public const int EcMatchGetTeamAttrFail = 30113; // 匹配获取队伍属性失败
        public const int EcMatchInnerLogicErr = -30150; // 匹配内部逻辑错误

        //帧同步服务类错误（40000-49999）
        public const int EcRelayAlreadyExists = 40000;    //重复创建
        public const int EcRelayNotExists = 40001;   //服务不存在
        public const int EcRelayDataExceedLimited = 40002;  //data长度超限制
        public const int EcRelayMemberAlreadyExists = 40003;    //成员已存在
        public const int EcRelayMemberNotExists = 40004;    //成员不存在
        public const int EcRelayStateInvalid = 40005;    //状态异常
        public const int EcRelayInvalidFrameRate = 40006;    //帧率非法
        public const int EcRelaySetFrameRateForbidden = 40007;    //开局状态下，G不允许修改帧率
        public const int EcRelayNoMembers = 40008;    //没任何成员
        public const int EcRelayGamesvrServiceNotOpen = 40009;    //自定义扩展服务（gamesvr）未开通
        public const int EcRelayReqPodFail = 40010;    //请求分配pod失败
        public const int EcRelayNoAvailablePod = 40011;    //无可用的pod
        public const int EcRelayGetFrameCacheFail = 40012;    //查询帧缓存失败
        public const int EcRelayHkvCacheError = 40015;    //共享内存缓存错误
        public const int EcRelayRedisCacheError = 40016;    //redis缓存错误
        public const int EcRelayNotifyRelayworkerFail = 40018;    //通知relayworker失败
        public const int EcRelayResetRelayRoomFail = 40019;    //重置房间对局失败
        public const int EcRelayCleanRelayRoomFail = 40020;    //清理房间对局数据失败
        public const int EcRelayNoPermission = 40100;    //没权限，401开头是权限相关错误
        public const int EcRelayNotifyGamesvrFail = 40200;    //通知自定义服务gamesvr失败， 402开头，是自定义gamesvr相关的错误
        public const int EcRelayForwardToGamesvrFail = 40201;    //转发到自定义逻辑svr失败
        public const int EcRelayForwardToClientFail = 40202;    //转发到client-sdk失败

        //50000～59999 调度中心
        //公共段参数错误 60000~65999
        public const int EcInvalidParams = 60000; // 业务参数错误
        ////匹配
        public const int EcInvalidParamsPlayModeVersion = 60001; // 玩法协议版本号错误
        public const int EcInvalidParamsPlayModeRuletype = 60002; // 玩法协议规则类型错误
        public const int EcInvalidParamsPlayModeExpression = 60003; // 玩法协议规则表达式错误
        public const int EcInvalidParamsPlayModeTeam = 60004; // 玩法协议规则团队表达式错误
        //// msg queue 参数错误
        public const int EcInvalidParamsMsgqEncode = 60020; // 消息队列 消息encode 参数错误
        public const int EcInvalidParamsMsgqDecode = 60021; // 消息队列 消息decode 参数错误
                                                                 ////CheckReq para check - 对外
        public const int EcInvalidParamsGameId = 61000; // 参数错误 game_id
        public const int EcInvalidParamsPlayerInfo = 61001; // 参数错误 player_info
        public const int EcInvalidParamsMaxPlayers = 61002; // 参数错误 max_players
        public const int EcInvalidParamsRoomType = 61003; // 参数错误 room_type
        public const int EcInvalidParamsPlayerId = 61004; // 参数错误 player_id
        public const int EcInvalidParamsMatchType = 61005; // 参数错误 match_type
        public const int EcInvalidParamsMatchCode = 61006; // 参数错误 match_code
        public const int EcInvalidParamsOpenId = 61007; // 参数错误 open_id
        public const int EcInvalidParamsPlatform = 61008; // 参数错误 platform
        public const int EcInvalidParamsTimestamp = 61009; // 参数错误 timestamp
        public const int EcInvalidParamsSign = 61010; // 参数错误 sign
        public const int EcInvalidParamsNonce = 61011; // 参数错误 nonce
        public const int EcInvalidParamsToken = 61012; // 参数错误 token
        public const int EcInvalidParamsNetworkState = 61013; // 参数错误 network_state
        public const int EcInvalidParamsRoomName = 61014; // 参数错误 room_name
        public const int EcInvalidParamsCreateRoomType = 61015; // 参数错误 create_room_type
        public const int EcInvalidParamsDeviceId = 61016; // 参数错误 device_id

        // 系统错误 -6600~-69999
        //myspp框架抛出-1000
        public const int EcMysppSystemErr = -1000;     //myspp框架返回-1000
                                                           ////redis
        public const int EcRedisKeyNotExist = -66000;   // redis KEY 不存在
        public const int EcRedisSetOpErr = -66001;   // redis set 类操作失败
        public const int EcRedisGetOpErr = -66002;   // redis get 类操作失败
        public const int EcRedisDelOpErr = -66003;   // redis del 类操作失败
        public const int EcRedisExpireOpErr = -66004;   // redis 操作异常
        public const int EcRedisLockOpErr = -66005;   // redis 加锁 类操作失败
        public const int EcRedisLockAlreadyExist = -66006;   // redis 加锁冲突 类操作失败
        public const int EcRedisListOpErr = -66020;   // redis list 操作失败
        public const int EcRedisListPopEmpty = -66021;   // redis list pop 空结果
        public const int EcRedisPoolGetInstanceFail = -66022;   // redis 实例池获取实例失败
        public const int EcRedisSetIsEmpty = -66023;   // redis set内为空
                                                            //mysql
        public const int EcMysqlNoRowFound = -66100;   // 查询为空
        public const int EcMysqlMultiRowFound = -66101;   // 查询为空
        public const int EcMysqlInsertFail = -66102;   // 插入失败
        public const int EcMysqlDeleteFail = -66103;   // 失败
        public const int EcMysqlUpdateFail = -66104;   // 失败
        public const int EcMysqlQuerysFail = -66105;   // 失败
                                                           ////pb
        public const int EcPbSerializeToStrErr = -66200;  // 序列化失败
        public const int EcPbParseFromStrErr = -66201;  // 反序列化失败
                                                              ////json
        public const int EcDataFormatErr = -66210;  // 数据格式转化失败
        public const int EcJsonFormatErr = -66211;  // JSON数据格式转化失败
        public const int EcJsonPlayModeFormatErr = -66212;  // 玩法数据格式转化失败
        public const int EcJsonPlayModePariseErr = -66213;  // 玩法数据格式转化失败
                                                                  //// 对内接口参数错误
        public const int EcInvalidParamsRecoreId = -66601;    //参数错误 recordId
                                                                   ////libs
        public const int EcHashidErr = -66700;    // hashcode生成失败
        public const int EcHashidEncodeErr = -66701;    // hashcode编码失败
        public const int EcHashidDecodeErr = -66702;    // hashcode解码失败
                                                            ////conf
        public const int EcConfRoomIdBucketErr = -66801;   // 配置 房间id管理模块错误
                                                                 //90000～99999 预留给客户端
                                                                 ////客户端错误
        public const int EcSdkSendFail = 90001; // 消息发送失败
        public const int EcSdkUninit = 90002; // SDK 未初始化
        public const int EcSdkResTimeout = 90003; // 消息响应超时
        public const int EcSdkNoLogin = 90004; // 登录态错误
        public const int EcSdkNoCheckLogin = 90005; // 帧同步鉴权错误
        public const int EcSdkSocketError = 90006; // 网络错误
        public const int EcSdkSocketClose = 90007; // Socket 断开
        public const int EcSdkNoRoom = 90008; // 无房间
    };
}