using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Utils : MonoBehaviour
{
	public static IPAddress GetIpV4Address(IPHostEntry entry)
	{
		foreach (IPAddress address in entry.AddressList)
		{
			if (address.AddressFamily == AddressFamily.InterNetwork)
				return address;
		}

		return null;
	}
}