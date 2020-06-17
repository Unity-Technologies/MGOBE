// Copyright (C) 2017 prototyped.cn All rights reserved.
// Distributed under the terms and conditions of the MIT License.
// See accompanying files LICENSE.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.unity.mgobe.src.Net.Kcp
{
    public class Kcp
    {
        public const int IkcpRtoNdl = 30;         // no delay min rto
        public const int IkcpRtoMin = 100;        // normal min rto
        public const int IkcpRtoDef = 200;
        public const int IkcpRtoMax = 60000;
        public const int IkcpCmdPush = 81;        // cmd: push data
        public const int IkcpCmdAck = 82;         // cmd: ack
        public const int IkcpCmdWask = 83;        // cmd: window probe (ask)
        public const int IkcpCmdWins = 84;        // cmd: window size (tell)
        public const int IkcpAskSend = 1;         // need to send IKCP_CMD_WASK
        public const int IkcpAskTell = 2;         // need to send IKCP_CMD_WINS
        public const int IkcpWndSnd = 32;
        public const int IkcpWndRcv = 32;
        public const int IkcpMtuDef = 1400;
        public const int IkcpAckFast = 3;
        public const int IkcpInterval = 100;
        public const int IkcpOverhead = 24;
        public const int IkcpDeadlink = 20;
        public const int IkcpThreshInit = 2;
        public const int IkcpThreshMin = 2;
        public const int IkcpProbeInit = 7000;    // 7 secs to probe window size
        public const int IkcpProbeLimit = 120000; // up to 120 secs to probe window

        public const int IkcpLogOutput = 0x1;
        public const int IkcpLogInput = 0x2;
        public const int IkcpLogSend = 0x4;
        public const int IkcpLogRecv = 0x8;
        public const int IkcpLogInData = 0x10;
        public const int IkcpLogInAck = 0x20;
        public const int IkcpLogInProbe = 0x40;
        public const int IkcpLogInWins = 0x80;
        public const int IkcpLogOutData = 0x100;
        public const int IkcpLogOutAck = 0x200;
        public const int IkcpLogOutProbe = 0x400;
        public const int IkcpLogOutWins = 0x800;


        // encode 8 bits unsigned int
        public static void ikcp_encode8u(byte[] p, int offset, byte c)
        {
            p[offset] = c;
        }

        // decode 8 bits unsigned int
        public static byte ikcp_decode8u(byte[] p, ref int offset)
        {
            return p[offset++];
        }

        // encode 16 bits unsigned int (lsb)
        public static void ikcp_encode16u(byte[] p, int offset, UInt16 v)
        {
            p[offset] = (byte)(v & 0xFF);
            p[offset + 1] = (byte)(v >> 8);
        }

        // decode 16 bits unsigned int (lsb)
        public static UInt16 ikcp_decode16u(byte[] p, ref int offset)
        {
            var pos = offset;
            offset += 2;
            return (UInt16)((UInt16)p[pos] | (UInt16)(p[pos + 1] << 8));
        }

        // encode 32 bits unsigned int (lsb)
        public static void ikcp_encode32u(byte[] p, int offset, UInt32 l)
        {
            p[offset] = (byte)(l & 0xFF);
            p[offset + 1] = (byte)(l >> 8);
            p[offset + 2] = (byte)(l >> 16);
            p[offset + 3] = (byte)(l >> 24);
        }

        // decode 32 bits unsigned int (lsb)
        public static UInt32 ikcp_decode32u(byte[] p, ref int offset)
        {
            var pos = offset;
            offset += 4;
            return ((UInt32)p[pos] | (UInt32)(p[pos + 1] << 8)
                | (UInt32)(p[pos + 2] << 16) | (UInt32)(p[pos + 3] << 24));
        }

        public static UInt32 _imin_(UInt32 a, UInt32 b)
        {
            return a <= b ? a : b;
        }

        public static UInt32 _imax_(UInt32 a, UInt32 b)
        {
            return a >= b ? a : b;
        }

        public static UInt32 _ibound_(UInt32 lower, UInt32 middle, UInt32 upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        public static Int32 _itimediff(UInt32 later, UInt32 earlier)
        {
            return (Int32)(later - earlier);
        }

        internal class Segment
        {
            internal UInt32 conv = 0;
            internal UInt32 cmd = 0;
            internal UInt32 frg = 0;
            internal UInt32 wnd = 0;
            internal UInt32 ts = 0;
            internal UInt32 sn = 0;
            internal UInt32 una = 0;
            internal UInt32 resendts = 0;
            internal UInt32 rto = 0;
            internal UInt32 faskack = 0;
            internal UInt32 xmit = 0;
            internal byte[] data;

            internal Segment(int size = 0)
            {
                data = new byte[size];
            }

            internal void Encode(byte[] ptr, ref int offset)
            {
                var len = (UInt32)data.Length;
                ikcp_encode32u(ptr, offset, conv);
                ikcp_encode8u(ptr, offset + 4, (byte)cmd);
                ikcp_encode8u(ptr, offset + 5, (byte)frg);
                ikcp_encode16u(ptr, offset + 6, (UInt16)wnd);
                ikcp_encode32u(ptr, offset + 8, ts);
                ikcp_encode32u(ptr, offset + 12, sn);
                ikcp_encode32u(ptr, offset + 16, una);
                ikcp_encode32u(ptr, offset + 20, len);
                offset += IkcpOverhead;
            }

        }

        readonly UInt32 _conv = 0;
        UInt32 _mtu = 0;
        UInt32 _mss = 0;
        UInt32 _state = 0;

        UInt32 _sndUna = 0;
        UInt32 _sndNxt = 0;
        UInt32 _rcvNxt = 0;

        UInt32 _tsRecent = 0;
        UInt32 _tsLastack = 0;
        UInt32 _ssthresh = 0;

        Int32 _rxRttval = 0;
        Int32 _rxSrtt = 0;
        Int32 _rxRto = 0;
        Int32 _rxMinrto = 0;

        UInt32 _sndWnd = 0;
        UInt32 _rcvWnd = 0;
        UInt32 _rmtWnd = 0;
        UInt32 _cwnd = 0;
        UInt32 _probe = 0;

        UInt32 _current = 0;
        UInt32 _interval = 0;
        UInt32 _tsFlush = 0;
        UInt32 _xmit = 0;

        UInt32 _nrcvBuf = 0;
        UInt32 _nsndBuf = 0;
        UInt32 _nrcvQue = 0;
        UInt32 _nsndQue = 0;

        UInt32 _nodelay = 0;
        UInt32 _updated = 0;
        UInt32 _tsProbe = 0;
        UInt32 _probeWait = 0;
        readonly UInt32 _deadLink = 0;
        UInt32 _incr = 0;

        readonly LinkedList<Segment> _sndQueue;
        readonly LinkedList<Segment> _rcvQueue;
        readonly LinkedList<Segment> _sndBuf;
        readonly LinkedList<Segment> _rcvBuf;

        UInt32[] _acklist;
        UInt32 _ackcount = 0;
        UInt32 _ackblock = 0;

        byte[] _buffer;
        readonly object _user;

        Int32 _fastresend = 0;
        Int32 _nocwnd = 0;

        public delegate void OutputDelegate(byte[] data, int size, object user);
        OutputDelegate _output;

        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection. 'user' will be passed to the output callback
        // output callback can be setup like this: 'kcp->output = my_udp_output'
        public Kcp(UInt32 conv, object user)
        {
            Debug.Assert(BitConverter.IsLittleEndian); // we only support little endian device

            _user = user;
            _conv = conv;
            _sndWnd = IkcpWndSnd;
            _rcvWnd = IkcpWndRcv;
            _rmtWnd = IkcpWndRcv;
            _mtu = IkcpMtuDef;
            _mss = _mtu - IkcpOverhead;
            _rxRto = IkcpRtoDef;
            _rxMinrto = IkcpRtoMin;
            _interval = IkcpInterval;
            _tsFlush = IkcpInterval;
            _ssthresh = IkcpThreshInit;
            _deadLink = IkcpDeadlink;
            _buffer = new byte[(_mtu + IkcpOverhead) * 3];
            _sndQueue = new LinkedList<Segment>();
            _rcvQueue = new LinkedList<Segment>();
            _sndBuf = new LinkedList<Segment>();
            _rcvBuf = new LinkedList<Segment>();
        }

        // release kcp control object
        public void Release()
        {
            _sndBuf.Clear();
            _rcvBuf.Clear();
            _sndQueue.Clear();
            _rcvQueue.Clear();
            _nrcvBuf = 0;
            _nsndBuf = 0;
            _nrcvQue = 0;
            _nsndQue = 0;
            _ackblock = 0;
            _ackcount = 0;
            _buffer = null;
            _acklist = null;
        }

        // set output callback, which will be invoked by kcp
        public void SetOutput(OutputDelegate output)
        {
            _output = output;
        }

        // user/upper level recv: returns size, returns below zero for EAGAIN
        public int Recv(byte[] buffer, int offset, int len)
        {
            var ispeek = (len < 0 ? 1 : 0);
            var recover = 0;

            if (_rcvQueue.Count == 0)
                return -1;

            if (len < 0)
                len = -len;

            var peeksize = PeekSize();
            if (peeksize < 0)
                return -2;

            if (peeksize > len)
                return -3;

            if (_nrcvQue >= _rcvWnd)
                recover = 1;

            // merge fragment
            len = 0;
            LinkedListNode<Segment> next = null;
            for (var node = _rcvQueue.First; node != null; node = next)
            {
                var fragment = 0;
                var seg = node.Value;
                next = node.Next;
                
                if (buffer != null)
                {
                    Buffer.BlockCopy(seg.data, 0, buffer, offset, seg.data.Length);
                    offset += seg.data.Length;
                }
                len += seg.data.Length;
                fragment = (int)seg.frg;

                Log(IkcpLogRecv, "recv sn={0}", seg.sn);

                if (ispeek == 0)
                {
                    _rcvQueue.Remove(node);
                    _nrcvQue--;
                }

                if (fragment == 0)
                    break;
            }

            Debug.Assert(len == peeksize);

            // move available data from rcv_buf -> rcv_queue
            while (_rcvBuf.Count > 0)
            {
                var node = _rcvBuf.First;
                var seg = node.Value;
                if (seg.sn == _rcvNxt && _nrcvQue < _rcvWnd)
                {
                    _rcvBuf.Remove(node);
                    _nrcvBuf--;
                    _rcvQueue.AddLast(node);
                    _nrcvQue++;
                    _rcvNxt++;
                }
                else
                {
                    break;
                }
            }

            // fast recover
            if (_nrcvQue < _rcvWnd && recover != 0)
            {
                // ready to send back IKCP_CMD_WINS in ikcp_flush
                // tell remote my window size
                _probe |= IkcpAskTell;
            }

            return len;
        }

        // check the size of next message in the recv queue
        public int PeekSize()
        {
            if (_rcvQueue.Count == 0)
                return -1;

            var node = _rcvQueue.First;
            var seg = node.Value;
            if (seg.frg == 0)
                return seg.data.Length;

            if (_nrcvQue < seg.frg + 1)
                return -1;

            var length = 0;
            for (node = _rcvQueue.First; node != null; node = node.Next)
            {
                seg = node.Value;
                length += seg.data.Length;
                if (seg.frg == 0)
                    break;
            }
            return length;
        }

        // user/upper level send, returns below zero for error
        public int Send(byte[] buffer, int offset, int len)
        {
            Debug.Assert(_mss > 0);
            if (len < 0)
                return -1;

            //
            // not implement streaming mode here as ikcp.c
            //

            var count = 0;
            if (len <= (int)_mss)
                count = 1;
            else
                count = (len + (int)_mss - 1) / (int)_mss;

            if (count > 255) // maximum value `frg` can present
                return -2;

            if (count == 0)
                count = 1;

            // fragment
            for (var i = 0; i < count; i++)
            {
                var size = len > (int)_mss ? (int)_mss : len;
                var seg = new Segment(size);
                if (buffer != null && len > 0)
                {
                    Buffer.BlockCopy(buffer, offset, seg.data, 0, size);
                    offset += size;
                }
                seg.frg = (UInt32)(count - i - 1);
                _sndQueue.AddLast(seg);
                _nsndQue++;
                len -= size;
            }
            return 0;
        }

        // parse ack
        void UpdateAck(Int32 rtt)
        {
            if (_rxSrtt == 0)
            {
                _rxSrtt = rtt;
                _rxRttval = rtt / 2;
            }
            else
            {
                var delta = rtt - _rxSrtt;
                if (delta < 0)
                    delta = -delta;

                _rxRttval = (3 * _rxRttval + delta) / 4;
                _rxSrtt = (7 * _rxSrtt + rtt) / 8;
                if (_rxSrtt < 1)
                    _rxSrtt = 1;
            }

            var rto = _rxSrtt + _imax_(_interval, (UInt32)(4 * _rxRttval));
            _rxRto = (Int32)_ibound_((UInt32)_rxMinrto, (UInt32)rto, IkcpRtoMax);
        }

        void ShrinkBuf()
        {
            var node = _sndBuf.First;
            if (node != null)
            {
                var seg = node.Value;
                _sndUna = seg.sn;
            }
            else
            {
                _sndUna = _sndNxt;
            }
        }

        void ParseAck(UInt32 sn)
        {
            if (_itimediff(sn, _sndUna) < 0 || _itimediff(sn, _sndNxt) >= 0)
                return;

            LinkedListNode<Segment> next = null;
            for (var node = _sndBuf.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (sn == seg.sn)
                {
                    _sndBuf.Remove(node);
                    _nsndBuf--;
                    break;
                }
                if (_itimediff(sn, seg.sn) < 0)
                    break;
            }
        }

        void ParseUna(UInt32 una)
        {
            LinkedListNode<Segment> next = null;
            for (var node = _sndBuf.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (_itimediff(una, seg.sn) > 0)
                {
                    _sndBuf.Remove(node);
                    _nsndBuf--;
                }
                else
                {
                    break;
                }
            }
        }

        void ParseFastAck(UInt32 sn)
        {
            if (_itimediff(sn, _sndUna) < 0 || _itimediff(sn, _sndNxt) >= 0)
                return;

            LinkedListNode<Segment> next = null;
            for (var node = _sndBuf.First; node != null; node = next)
            {
                var seg = node.Value;
                next = node.Next;
                if (_itimediff(sn, seg.sn) < 0)
                {
                    break;
                }
                else if (sn != seg.sn)
                {
                    seg.faskack++;
                }
            }
        }

        // ack append
        void AckPush(UInt32 sn, UInt32 ts)
        {
            var newsize = _ackcount + 1;
            if (newsize > _ackblock)
            {
                UInt32 newblock = 8;
                for (; newblock < newsize; newblock <<= 1)
                    ;

                var acklist = new UInt32[newblock * 2];
                if (_acklist != null)
                {
                    for (var i = 0; i < _ackcount; i++)
                    {
                        acklist[i * 2] = _acklist[i * 2];
                        acklist[i * 2 + 1] = _acklist[i * 2 + 1];
                    }
                }
                _acklist = acklist;
                _ackblock = newblock;
            }
            _acklist[_ackcount * 2] = sn;
            _acklist[_ackcount * 2 + 1] = ts;
            _ackcount++;
        }

        void AckGet(int pos, ref UInt32 sn, ref UInt32 ts)
        {
            sn = _acklist[pos * 2];
            ts = _acklist[pos * 2 + 1];
        }

        // parse data
        void ParseData(Segment newseg)
        {
            var sn = newseg.sn;
            var repeat = 0;

            if (_itimediff(sn, _rcvNxt + _rcvWnd) >= 0 ||
                _itimediff(sn, _rcvNxt) < 0)
            {
                return;
            }

            LinkedListNode<Segment> node = null;
            LinkedListNode<Segment> prev = null;
            for (node = _rcvBuf.Last; node != null; node = prev)
            {
                var seg = node.Value;
                prev = node.Previous;
                if (seg.sn == sn)
                {
                    repeat = 1;
                    break;
                }
                if (_itimediff(sn, seg.sn) > 0) 
                {
                    break;
                }
            }
            if (repeat == 0)
            {
                if (node != null)
                {
                    _rcvBuf.AddAfter(node, newseg);
                }
                else
                {
                    _rcvBuf.AddFirst(newseg);
                }
                _nrcvBuf++;
            }

            // move available data from rcv_buf -> rcv_queue
            while (_rcvBuf.Count > 0)
            {
                node = _rcvBuf.First;
                var seg = node.Value;
                if (seg.sn == _rcvNxt && _nrcvQue < _rcvWnd)
                {
                    _rcvBuf.Remove(node);
                    _nrcvBuf--;
                    _rcvQueue.AddLast(node);
                    _nrcvQue++;
                    _rcvNxt++;
                }
                else
                {
                    break;
                }
            }
        }

        // when you received a low level packet (eg. UDP packet), call it
        public int Input(byte[] data, int offset, int size)
        {
            UInt32 maxack = 0;
            var flag = 0;

            Log(IkcpLogInput, "[RI] {0} bytes", size);

            if (data == null || size < IkcpOverhead)
                return -1;

            while (true)
            {
                if (size < IkcpOverhead)
                    break;

                var conv = ikcp_decode32u(data, ref offset);
                if (_conv != conv)
                    return -1;
                UInt32 cmd = ikcp_decode8u(data, ref offset);
                UInt32 frg = ikcp_decode8u(data, ref offset);
                UInt32 wnd = ikcp_decode16u(data, ref offset);
                var ts = ikcp_decode32u(data, ref offset);
                var sn = ikcp_decode32u(data, ref offset);
                var una = ikcp_decode32u(data, ref offset);
                var len = ikcp_decode32u(data, ref offset);

                size -= IkcpOverhead;
                if (size < len)
                    return -2;

                if (cmd != IkcpCmdPush && cmd != IkcpCmdAck &&
                    cmd != IkcpCmdWask && cmd != IkcpCmdWins)
                    return -3;

                _rmtWnd = wnd;
                ParseUna(una);
                ShrinkBuf();

                if (cmd == IkcpCmdAck)
                {
                    if (_itimediff(_current, ts) >= 0)
                    {
                        UpdateAck(_itimediff(_current, ts));
                    }
                    ParseAck(sn);
                    ShrinkBuf();
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                    }
                    else
                    {
                        if (_itimediff(sn, maxack) > 0)
                        {
                            maxack = sn;
                        }
                    }
                    Log(IkcpLogInData, "input ack: sn={0} rtt={1} rto={2}",
                        sn, _itimediff(_current, ts), _rxRto);
                }
                else if (cmd == IkcpCmdPush)
                {
                    Log(IkcpLogInData, "input psh: sn={0} ts={1}", sn, ts);
                    if (_itimediff(sn, _rcvNxt + _rcvWnd) < 0)
                    {
                        AckPush(sn, ts);
                        if (_itimediff(sn, _rcvNxt) >= 0)
                        {
                            var seg = new Segment((int)len);
                            seg.conv = conv;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            if (len > 0)
                            {
                                Buffer.BlockCopy(data, offset, seg.data, 0, (int)len);
                            }
                            ParseData(seg);
                        }
                    }
                }
                else if (cmd == IkcpCmdWask)
                {
                    // ready to send back IKCP_CMD_WINS in ikcp_flush
                    // tell remote my window size
                    _probe |= IkcpAskTell;
                    Log(IkcpLogInProbe, "input probe");
                }
                else if (cmd == IkcpCmdWins)
                {
                    // do nothing
                    Log(IkcpLogInWins, "input wins: {0}", wnd);
                }
                else
                {
                    return -3;
                }

                offset += (int)len;
                size -= (int)len;
            }

            if (flag != 0)
            {
                ParseFastAck(maxack);
            }

            var unack = _sndUna;
            if (_itimediff(_sndUna, unack) > 0)
            {
                if (_cwnd < _rmtWnd)
                {
                    if (_cwnd < _ssthresh)
                    {
                        _cwnd++;
                        _incr += _mss;
                    }
                    else
                    {
                        if (_incr < _mss)
                            _incr = _mss;
                        _incr += (_mss * _mss) / _incr + (_mss / 16);
                        if ((_cwnd + 1) * _mss <= _incr)
                            _cwnd++;
                    }
                    if (_cwnd > _rmtWnd)
                    {
                        _cwnd = _rmtWnd;
                        _incr = _rmtWnd * _mss;
                    }
                }
            }

            return 0;
        }

        int WndUnused()
        {
            if (_nrcvQue < _rcvWnd)
                return (int)(_rcvWnd - _nrcvQue);
            return 0;
        }

        // flush pending data
        void Flush()
        {
            var change = 0;
            var lost = 0;
            var offset = 0;

            // 'ikcp_update' haven't been called. 
            if (_updated == 0)
                return;

            var seg = new Segment
            {
                conv = _conv,
                cmd = IkcpCmdAck,
                wnd = (UInt32)WndUnused(),
                una = _rcvNxt,
            };

            // flush acknowledges
            var count = (int)_ackcount;
            for (var i = 0; i < count; i++)
            {
                if ((offset + IkcpOverhead) > _mtu)
                {
                    _output(_buffer, offset, _user);
                    offset = 0;
                }
                AckGet(i, ref seg.sn, ref seg.ts);
                seg.Encode(_buffer, ref offset);
            }

            _ackcount = 0;

            // probe window size (if remote window size equals zero)
            if (_rmtWnd == 0)
            {
                if (_probeWait == 0)
                {
                    _probeWait = IkcpProbeInit;
                    _tsProbe = _current + _probeWait;
                }
                else
                {
                    if (_itimediff(_current, _tsProbe) >= 0)
                    {
                        if (_probeWait < IkcpProbeInit)
                            _probeWait = IkcpProbeInit;
                        _probeWait += _probeWait / 2;
                        if (_probeWait > IkcpProbeLimit)
                            _probeWait = IkcpProbeLimit;
                        _tsProbe = _current + _probeWait;
                        _probe |= IkcpAskSend;
                    }
                }
            }
            else
            {
                _tsProbe = 0;
                _probeWait = 0;
            }

            // flush window probing commands
            if ((_probe & IkcpAskSend) > 0)
            {
                seg.cmd = IkcpCmdWask;
                if ((offset + IkcpOverhead) > _mtu)
                {
                    _output(_buffer, offset, _user);
                    offset = 0;
                }
                seg.Encode(_buffer, ref offset);
            }

            // flush window probing commands
            if ((_probe & IkcpAskTell) > 0)
            {
                seg.cmd = IkcpCmdWins;
                if ((offset + IkcpOverhead) > _mtu)
                {
                    _output(_buffer, offset, _user);
                    offset = 0;
                }
                seg.Encode(_buffer, ref offset);
            }

            _probe = 0;

            // calculate window size
            var cwnd = _imin_(_sndWnd, _rmtWnd);
            if (_nocwnd == 0)
                cwnd = _imin_(_cwnd, cwnd);

            // move data from snd_queue to snd_buf
            while (_itimediff(_sndNxt, _sndUna + cwnd) < 0)
            {
                if (_sndQueue.Count == 0)
                    break;

                var node = _sndQueue.First;
                var newseg = node.Value;
                _sndQueue.Remove(node);
                _sndBuf.AddLast(node);
                _nsndQue--;
                _nsndBuf++;

                newseg.conv = _conv;
                newseg.cmd = IkcpCmdPush;
                newseg.wnd = seg.wnd;
                newseg.ts = _current;
                newseg.sn = _sndNxt++;
                newseg.una = _rcvNxt;
                newseg.resendts = _current;
                newseg.rto = (UInt32)_rxRto;
                newseg.faskack = 0;
                newseg.xmit = 0;
            }

            // calculate resent
            var resent = (_fastresend > 0 ? (UInt32)_fastresend : 0xffffffff);
            var rtomin = (_nodelay == 0 ? (UInt32)(_rxRto >> 3) : 0);

            // flush data segments
            for (var node = _sndBuf.First; node != null; node = node.Next)
            {
                var segment = node.Value;
                var needsend = 0;
                if (segment.xmit == 0)
                {
                    needsend = 1;
                    segment.xmit++;
                    segment.rto = (UInt32)_rxRto;
                    segment.resendts = _current + segment.rto + rtomin;
                }
                else if (_itimediff(_current, segment.resendts) >= 0)
                {
                    needsend = 1;
                    segment.xmit++;
                    _xmit++;
                    if (_nodelay == 0)
                        segment.rto += (UInt32)_rxRto;
                    else
                        segment.rto += (UInt32)_rxRto / 2;
                    segment.resendts = _current + segment.rto;
                    lost = 1;
                }
                else if (segment.faskack >= resent)
                {
                    needsend = 1;
                    segment.xmit++;
                    segment.faskack = 0;
                    segment.resendts = _current + segment.rto;
                    change++;
                }

                if (needsend > 0)
                {
                    segment.ts = _current;
                    segment.wnd = seg.wnd;
                    segment.una = _rcvNxt;

                    var need = IkcpOverhead;
                    if (segment.data != null)
                        need += segment.data.Length;

                    if (offset + need > _mtu)
                    {
                        _output(_buffer, offset, _user);
                        offset = 0;
                    }
                    segment.Encode(_buffer, ref offset);
                    if (segment.data.Length > 0)
                    {
                        Buffer.BlockCopy(segment.data, 0, _buffer, offset, segment.data.Length);
                        offset += segment.data.Length;
                    }
                    if (segment.xmit >= _deadLink)
                        _state = 0xffffffff;
                }
            }

            // flush remain segments
            if (offset > 0)
            {
                _output(_buffer, offset, _user);
                offset = 0;
            }

            // update ssthresh
            if (change > 0)
            {
                var inflight = _sndNxt - _sndUna;
                _ssthresh = inflight / 2;
                if (_ssthresh < IkcpThreshMin)
                    _ssthresh = IkcpThreshMin;
                _cwnd = _ssthresh + resent;
                _incr = _cwnd * _mss;
            }

            if (lost > 0)
            {
                _ssthresh = cwnd / 2;
                if (_ssthresh < IkcpThreshMin)
                    _ssthresh = IkcpThreshMin;
                _cwnd = 1;
                _incr = _mss;
            }

            if (_cwnd < 1)
            {
                _cwnd = 1;
                _incr = _mss;
            }
        }

        // update state (call it repeatedly, every 10ms-100ms), or you can ask 
        // ikcp_check when to call it again (without ikcp_input/_send calling).
        // 'current' - current timestamp in millisec. 
        public void Update(UInt32 current)
        {
            _current = current;
            if (_updated == 0)
            {
                _updated = 1;
                _tsFlush = current;
            }

            var slap = _itimediff(_current, _tsFlush);
            if (slap >= 10000 || slap < -10000)
            {
                _tsFlush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                _tsFlush += _interval;
                if (_itimediff(_current, _tsFlush) >= 0)
                    _tsFlush = _current + _interval;

                Flush();
            }
        }

        // Determine when should you invoke ikcp_update:
        // returns when you should invoke ikcp_update in millisec, if there 
        // is no ikcp_input/_send calling. you can call ikcp_update in that
        // time, instead of call update repeatly.
        // Important to reduce unnacessary ikcp_update invoking. use it to 
        // schedule ikcp_update (eg. implementing an epoll-like mechanism, 
        // or optimize ikcp_update when handling massive kcp connections)
        public UInt32 Check(UInt32 current)
        {
            var tsFlush = _tsFlush;
            var tmFlush = 0x7fffffff;
            var tmPacket = 0x7fffffff;

            if (_updated == 0)
                return current;

            if (_itimediff(current, tsFlush) >= 10000 || 
                _itimediff(current, tsFlush) < -10000)
            {
                tsFlush = current;
            }

            if (_itimediff(current, tsFlush) >= 0)
                return current;

            tmFlush = _itimediff(tsFlush, current);

            for (var node = _sndBuf.First; node != null; node = node.Next)
            {
                var seg = node.Value;
                var diff = _itimediff(seg.resendts, current);
                if (diff <= 0)
                    return current;

                if (diff < tmPacket)
                    tmPacket = diff;
            }

            var minimal = (UInt32)(tmPacket < tmFlush ? tmPacket : tmFlush);
            if (minimal >= _interval)
                minimal = _interval;

            return current + minimal;
        }

        // change MTU size, default is 1400
        public int SetMtu(int mtu)
        {
            if (mtu < 50 || mtu < IkcpOverhead)
                return -1;

            var buffer = new byte[(mtu + IkcpOverhead) * 3];
            _mtu = (UInt32)mtu;
            _mss = _mtu - IkcpOverhead;
            _buffer = buffer;
            return 0;
        }

        public int Interval(int interval)
        {
            if (interval > 5000)
                interval = 5000;
            else if (interval < 10)
                interval = 10;

            _interval = (UInt32)interval;
            return 0;
        }

        // fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        // nodelay: 0:disable(default), 1:enable
        // interval: internal update timer interval in millisec, default is 100ms 
        // resend: 0:disable fast resend(default), 1:enable fast resend
        // nc: 0:normal congestion control(default), 1:disable congestion control
        public int NoDelay(int nodelay, int interval, int resend, int nc)
        {
            if (nodelay >= 0)
            {
                _nodelay = (UInt32)nodelay;
                if (nodelay > 0)
                {
                    _rxMinrto = IkcpRtoNdl;
                }
                else
                {
                    _rxMinrto = IkcpRtoMin;
                }
            }
            if (interval >= 0)
            {
                if (interval > 5000)
                    interval = 5000;
                else if (interval < 10)
                    interval = 10;

                _interval = (UInt32)interval;
            }

            if (resend >= 0)
                _fastresend = resend;

            if (nc >= 0)
                _nocwnd = nc;

            return 0;
        }

        // set maximum window size: sndwnd=32, rcvwnd=32 by default
        public int WndSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0)
                _sndWnd = (UInt32)sndwnd;
            if (rcvwnd > 0)
                _rcvWnd = (UInt32)rcvwnd;
            return 0;
        }

        // get how many packet is waiting to be sent
        public int WaitSnd()
        {
            return (int)(_nsndBuf + _nsndQue);
        }

        // read conv
        public UInt32 GetConv()
        {
            return _conv;
        }

        public UInt32 GetState()
        {
            return _state;
        }

        public void SetMinRto(int minrto)
        {
            _rxMinrto = minrto;
        }

        public void SetFastResend(int resend)
        {
            _fastresend = resend;
        }

        void Log(int mask, string format, params object[] args)
        {
            // Console.WriteLine(mask + String.Format(format, args));
        }
    }
}
