using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.UI;

public class SendPos : MonoBehaviour {
    //socket缓冲区
    Socket socket;
    const int BUFFER_SIZE = 1024;
    public byte[] readBuff = new byte[BUFFER_SIZE];
    //玩家列表
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    //消息列表
    List<string> msgList = new List<string>();
    //player预设体
    GameObject prefab;
    //自己的ID，ip端口
    string id;


    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
	void AddPlayer(string id,Vector3 pos)
    {
        GameObject player = Instantiate(prefab, pos, Quaternion.identity);
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id;
        players.Add(id, player);
    }
    /// <summary>
    /// 发送位置协议
    /// </summary>
    void SendPosition()
    {
        GameObject player = players[id];
        Vector3 pos = player.transform.position;
        //组装协议
        string str = "POS ";
        str += id + " ";
        str += pos.x.ToString() + " ";
        str += pos.y.ToString() + " ";
        str += pos.z.ToString() + " ";

        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);

    }
    /// <summary>
    /// 发送离开协议
    /// </summary>
    void SendLeave()
    {
        string str = "LEAVE ";
        str += id + " ";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
    }
    /// <summary>
    /// 移动
    /// </summary>
    void Move()
    {
        if (id == "")
            return;

        GameObject player = players[id];

        if(Input.GetKey (KeyCode.UpArrow ))
        {
            player.transform.position += Vector3.forward;
            SendPosition();
        }
    }
    /// <summary>
    /// 离开
    /// </summary>
    void OnDestory()
    {
        SendLeave();
    }

    void Start()
    {
        Connect();
        //请求其他玩家列表
        //把自己放在随机位置
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
        float x = 100 + UnityEngine.Random.Range(-30, 30);
        float y = 0;
        float z = 100 + UnityEngine.Random.Range(-30, 30);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(id, pos);
        //同步
        SendPosition();
    }
    /// <summary>
    /// 连接
    /// </summary>
    void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect("127.0.0.1", 1234);
        id = socket.LocalEndPoint.ToString();

        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
    }

    void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count =socket.EndReceive (ar);
            //数据处理
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            msgList.Add(str);
            //继续接受
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        catch (Exception e)
        {

            socket.Close ();
        }
    }

    void Update()
    {
        //处理消息列表
        for (int i = 0; i < msgList.Count; i++)
        {
            HandleMsg();
        }
        Move();
    }
    /// <summary>
    /// 处理消息列表
    /// </summary>
    void HandleMsg()
    {
        if(msgList.Count<=0)
        {
            return;
        }
        string str = msgList[0];
        msgList.RemoveAt(0);
        string[] args = str.Split(' ');
        if(args[0]=="POS")
        {
            OnRecvPos(args[1], args[2], args[3], args[4]);
        }
        if(args[0]=="LEAVE")
        {
            OnRecvLeave(args[1]);
        }
    }

    void OnRecvPos(string id,string xStr,string yStr,string zStr)
    {
        //不更新自己
        if(id==this.id)
        {
            return;
        }
        //解析协议
        float x = float.Parse(xStr);
        float y = float.Parse(yStr);
        float z = float.Parse(zStr);
        Vector3 pos = new Vector3(x, y, z);
        //已经初始化的玩家
        if(players.ContainsKey (id))
        {
            players[id].transform.position = pos;
        }
        //尚未初始化的玩家
        else
        {
            AddPlayer(id, pos);
        }
    }

    void OnRecvLeave(string id)
    {
        if(players.ContainsKey (id))
        {
            Destroy(players[id]);
            players[id] = null;
        }
    }
}
