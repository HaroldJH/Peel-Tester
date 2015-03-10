using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialProgram2
{
    /// <summary>
    /// Serial 통신에서 받아들인 데이터들을 1.잃어버리지 않고, 2.쉽게 다루기 위해서
    /// 작성되었다.
    /// 데이터 추가하는 Add함수와 원하는 데이터가 들어왔는지 체크하는 Match함수, 그리고
    /// 원하는데이터를 사용하고 난 후 Buffer에서 지워버리는 Remove함수로 되어 있다.
    /// 또 Queue와 같이 동작하기 위해서 Enqueue,Dequeue 관련 함수를 두었다.
    /// </summary>
    public class Rs232Buffer
    {
        #region 멤버데이터
        protected List<byte> _list;
        protected int _startMatch;//매치 시작index
        protected int _endMatch; //매치끝Index
        #endregion

        #region 프로퍼티
        public int Count { get { return _list.Count(); } }
        public int StartMatchIndex { get { return _startMatch; } }
        public int EndMatchIndex { get { return _endMatch; } }
        public bool IsMatch { get { return (_startMatch > -1 && _endMatch > -1); } }
        public int MatchedByteCount { get { return _endMatch - _startMatch + 1; } }
        public bool IsEmpty { get { return _list.Count == 0; } }
        public byte this[int index]
        {
            get
            {
                return _list[index];
            }
        }
        public int LastIndex
        {
            get { return _list.Count() - 1; }
        }
        #endregion

        #region Private 멤버함수들
        private void InitializeMatchVariables()
        {
            _startMatch = _endMatch = -1;
        }
        /// <summary>
        /// startindex에서 부터 버퍼의 내용이 bytes와 같은지 체크 같으면 true
        /// </summary>
        /// <param name="startindex"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool IsEqual(int startindex, byte[] bytes)
        {
            if (startindex >= _list.Count) return false;
            if (startindex + bytes.Count() > _list.Count) return false;
            for (int i = 0; i < bytes.Count(); i++)
            {
                if (_list[startindex + i] != bytes[i]) return false;
            }
            return true;
        }
        #endregion

        #region Public 멤버함수
        public void Add(byte b)
        {
            _list.Add(b);
        }
        public void Add(byte[] bytes)
        {
            _list.AddRange(bytes);
        }
        /// <summary>
        /// bytes의 처음부터 count만큼 Add한다
        /// </summary>
        public void Add(byte[] bytes, int count)
        {
            int maxcount = bytes.Length > count ? count : bytes.Length;
            for (int i = 0; i < maxcount; i++)
            {
                _list.Add(bytes[i]);
            }
        }
        public void Add(string hexstring)
        {
            Add(Rs232Utils.HexStringToByteArray(hexstring));
        }

        /// <summary>
        /// startIndex에서부터 매치가 되는지 체크해서 매치가 되면 
        /// </summary>
        public bool MatchWithStartIndex(int startindex, byte b)
        {
            InitializeMatchVariables();
            if (startindex >= _list.Count) return false;
            InitializeMatchVariables();
            for (int i = startindex; i < _list.Count; i++)
            {
                if (b == _list[i])
                {
                    _startMatch = _endMatch = i;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// byte 배열이 매치되는 위치를 찾는다.
        /// ABCDEF이고 AB를 찾는다면 start 위치는 0 end는 1이다
        /// 
        /// </summary>
        public bool MatchWithStartIndex(int startindex, byte[] bytes)
        {
            InitializeMatchVariables();
            for (int i = startindex; i < _list.Count; i++)
            {
                if (IsEqual(i, bytes))
                {
                    _startMatch = i;
                    _endMatch = _startMatch + bytes.Count() - 1;
                    return true;
                }
            }
            InitializeMatchVariables();
            return false;
        }
        /// <summary>
        /// 한개의 바이트와 매치되는 것을 찾는다.
        /// _startMatchIndex와 endMatchIndex는 같다
        /// Match(0x30)과 같이 사용
        /// </summary>
        public bool Match(byte b)
        {
            return MatchWithStartIndex(0, b);
        }
        /// <summary>
        /// startbyte와 endbyte 가 있는 패턴을 찾는다
        /// Match(0x02,0x03)과 같이 사용한다
        /// </summary>
        public bool Match(byte startbyte, byte endbyte)
        {
            InitializeMatchVariables();
            bool flag = false;
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i] == startbyte)
                {
                    flag = true;
                    _startMatch = i;
                }
                if (flag && _list[i] == endbyte)
                {
                    _endMatch = i;
                    return true;
                }
            }
            InitializeMatchVariables();
            return false;
        }
        /// <summary>
        /// startbyte와 endbyte 가 있는 패턴을 찾는다
        /// Match(new byte[]{0x01,0x02}, new byte[] {0x34,0x35})과 같이 사용한다
        /// </summary>
        public bool Match(byte[] startbytes, byte[] endbytes)
        {
            InitializeMatchVariables();
            bool flag = false;
            for (int i = 0; i < _list.Count; i++)
            {
                if (IsEqual(i, startbytes))
                {
                    flag = true;
                    _startMatch = i;
                }
                if (flag && IsEqual(i, endbytes))
                {
                    _endMatch = i + endbytes.Count() - 1;
                    return true;
                }
            }
            InitializeMatchVariables();
            return false;
        }
        /// <summary>
        /// headbyte가있고 뒤게 어떤 바이트이던지 count만큼 갯수가 존재하는 패턴을 찾는다
        /// count는 headbyte를 포함하지 않는다.
        /// </summary>
        public bool MatchByteAndCount(byte headByte, int count)
        {
            InitializeMatchVariables();
            for (int i = 0; i < _list.Count; i++)
            {
                if (headByte == _list[i])
                {
                    _startMatch = i; break;
                }
            }
            //찾는 바이트가 없다
            if (_startMatch < 0)
            {
                InitializeMatchVariables(); return false;
            }
            if ((_startMatch + count) < _list.Count)
            {
                _endMatch = _startMatch + count;
                return true;
            }
            InitializeMatchVariables();
            return false;
        }
        /// <summary>
        /// headbyte가있고 뒤게 어떤 바이트이던지 count만큼 갯수가 존재하는 패턴을 찾는다
        /// count는 headbyte를 포함하지 않는다.
        /// </summary>
        public bool MatchByteArrayAndCount(byte[] headByte, int count)
        {
            InitializeMatchVariables();
            if (headByte.Length < 1) throw new Exception("in MatchByteArrayAndCount , bytearray should not null");
            if (count < 1) throw new Exception("in MatchByteArrayAndCount , count should over 0");

            for (int i = 0; i < _list.Count; i++)
            {
                if (IsEqual(i, headByte))
                {
                    _startMatch = i;
                    _endMatch = i + headByte.Length + count - 1;
                    if (_endMatch >= _list.Count)
                    {
                        _startMatch = -1; _endMatch = -1;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            InitializeMatchVariables();
            return false;
        }
        /// <summary>
        /// 찾고자하는 바이트 앞뒤에 갯수가 존재하면 match가 된다
        /// </summary>
        /// <param name="prevCount">찾고자하는 바이트앞에 존재하는 갯수</param>
        /// <param name="matchbyte">찾고자하는 바이트</param>
        /// <param name="postCount">찾고자하는 바이트뒤에 존재하는 갯수</param>
        /// <returns></returns>
        public bool Match(int prevCount, byte matchbyte, int postCount)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i] == matchbyte)
                {
                    int prev = i - prevCount;
                    int post = i + postCount;
                    if (i >= 0 && i < _list.Count)
                    {
                        _startMatch = prev;
                        _endMatch = post;
                        return true;
                    }
                }
            }
            InitializeMatchVariables();
            return false;

        }
        /// <summary>
        /// hexstring 02 ?? ?? 03 과 같이 표현된 매치를 찾는다
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        //public bool Match(string hex)
        //{
        //    InitializeMatchVariables();

        //    string[] strArray = hex.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        //    if (_list.Count < hex.Length) return false; 
        //    for (int i = 0; i < (_list.Count-hex.Length)+1; i++)
        //    {
        //        for(int j=0;i<
        //    }
        //    ByteUtils.HexToByte(hex);
        //}
        /// <summary>
        /// startbyte와 endbyte사이의 갯수가 count이면 매치된다
        /// </summary>
        /// <param name="startbyte">찾고자하는 시작바이트</param>
        /// <param name="endbyte">찾고자하는 마지막바이트</param>
        /// <param name="count">시작바이트와 마지막바이트 사이의 갯수,시작과 마지막바이트는 포함되지 않는다</param>
        /// <returns></returns>
        public bool MatchWithCount(byte startbyte, byte endbyte, int matchedCount)
        {
            bool startmatched = false;
            int cnt = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i] == startbyte)
                {
                    startmatched = true;
                    _startMatch = i;
                    cnt = 1;
                    continue;
                }
                if (startmatched)
                {
                    cnt++;
                }
                if (_list[i] == endbyte)
                {
                    if (cnt == matchedCount)
                    {
                        _endMatch = i;
                        return true;
                    }
                    else
                    {
                        cnt = 0;
                        startmatched = false;
                    }
                }
            }
            InitializeMatchVariables();
            return false;
        }

        /// <summary>
        /// 매치된 부분을 리턴한다 Start End 매치 바이트도 포함한다
        /// </summary>
        public byte[] GetMatchedBytes()
        {
            byte[] temp = new byte[MatchedByteCount];
            _list.CopyTo(_startMatch, temp, 0, MatchedByteCount);
            return temp;
        }

        public bool Match(byte[] bytes)
        {
            return MatchWithStartIndex(0, bytes);
        }
        public int CountOf(byte b)
        {
            int cnt = 0;
            foreach (byte bb in _list)
            {
                if (bb == b) cnt++;
            }
            return cnt;
        }
        public int IndexOf(byte b)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i] == b) return i;
            }
            return -1;
        }
        public int IndexOf(byte b, int seq)
        {
            if (seq < 1) throw new Exception("invalid seq number in IndexOf");
            int meet = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i] == b)
                {
                    meet++;
                    if (seq == meet) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 시작 인덱스 이전것들을 모두 지운다
        /// 갯수를 넘으면 모두 지운다
        /// </summary>
        /// <param name="startIndex"></param>
        public void RemoveBefore(int startIndex)
        {
            if (startIndex > _list.Count)
            {
                _list.Clear();
            }
            else
            {
                _list.RemoveRange(0, startIndex);
            }
        }
        /// <summary>
        /// endIndex의 뒷부분을 지운다. 자신부분은 지우지 않는다
        /// </summary>
        public void RemoveAfter(int endIndex)
        {
            if ((endIndex + 1) >= _list.Count()) return;
            _list.RemoveRange(endIndex + 1, _list.Count() - (endIndex + 1));
        }
        /// <summary>
        /// 매치된 부분을 모두 지운다
        /// </summary>
        public void RemoveMatched()
        {
            if (IsMatch)
            {
                _list.RemoveRange(_startMatch, (_endMatch - _startMatch) + 1);
                InitializeMatchVariables();
            }
        }
        public void RemoveRange(int startIndex, int endIndex)
        {
            int sidx = startIndex;
            int eidx = endIndex;
            if (_list.Count < 1) return;
            if (sidx > eidx) return;
            if (sidx < 0) sidx = 0;
            if (eidx >= _list.Count) eidx = _list.Count - 1;

            _list.RemoveRange(sidx, (eidx - sidx) + 1);
        }
        public void Clear() { _list.Clear(); _endMatch = _startMatch = -1; }

        /// <summary>
        /// startIndex에서 endIndex까지를 가져와서 배열로 리턴한다
        /// </summary>
        public byte[] CopyFrom(int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex >= _list.Count) return null;
            if (endIndex < startIndex) return null;
            byte[] temp = new byte[endIndex - startIndex + 1];
            //_list.CopyTo(_startMatch, temp, 0, MatchedByteCount);
            _list.CopyTo(startIndex, temp, 0, endIndex - startIndex + 1);
            return temp;
        }
        public byte[] AllBytes()
        {
            return _list.ToArray();
        }
        #region Enqueue & Dequeue 동작
        public void EnqueueByte(byte b)
        {
            Add(b);
        }
        public void EnqueueByteArray(byte[] bytes)
        {
            Add(bytes);
        }
        public void EnqueueHexString(string hexstring)
        {
            Add(hexstring);
        }
        public byte DequeueByte()
        {
            if (_list.Count < 1)
            {
                throw new Exception("No data in rs232buffer");
            }
            byte b = _list[0];
            _list.RemoveAt(0);
            return b;
        }
        public byte[] DequeueByteArray(int count)
        {
            if (count > _list.Count)
            {
                throw new Exception("Dequeue count larger than buffer count");
            }
            byte[] dqbytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                dqbytes[i] = _list[i];
            }
            _list.RemoveRange(0, count);
            return dqbytes;
        }
        #endregion

        public string ToHexString()
        {
            return Rs232Utils.ByteArrayToHexString(_list.ToArray());
        }
        //public string ToAsciiString()
        //{
        //    return Rs232Utils.ByteArrayToAsciiString(_list.ToArray());
        //}
        public override string ToString()
        {
            return string.Format("Match Start,End Index : {0},{1} \r\n Data:[{2}]", _startMatch, _endMatch, ToHexString());
        }

        #endregion

        #region 생성자
        public Rs232Buffer()
        {
            _list = new List<byte>();
            InitializeMatchVariables();
        }
        #endregion
    }
}
