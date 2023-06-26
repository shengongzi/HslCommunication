﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication.Profinet.Melsec;
using HslCommunication;
using System.Xml.Linq;
using HslCommunicationDemo.Control;
using HslCommunication.MQTT;
using HslCommunicationDemo.DemoControl;
using HslCommunicationDemo.PLC.Melsec;
using HslCommunication.Profinet.Toyota;
using HslCommunication.Core;
using HslCommunication.Profinet.Melsec.Helper;

namespace HslCommunicationDemo
{
	public partial class FormToyoPuc : HslFormContent
	{
		public FormToyoPuc( )
		{
			InitializeComponent( );
			toyota = new ToyoPuc( );
		}


		private ToyoPuc toyota = null;
		private AddressExampleControl addressExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;
			Language( Program.Language );


			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.Toyota.Helper.GetToyotaAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );
		}

		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "ToyoPuc Read PLC Demo";

				label1.Text = "Ip:";
				label3.Text = "Port:";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
			}
			else
			{

			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}

		#region Connect And Close



		private async void button1_Click( object sender, EventArgs e )
		{
			toyota.IpAddress = textBox_ip.Text;
			if (!int.TryParse( textBox_port.Text, out int port ))
			{
				MessageBox.Show( DemoUtils.PortInputWrong );
				return;
			}

			toyota.Port = port;

			toyota.ConnectClose( );
			toyota.LogNet = LogNet;
			button1.Enabled = false;
			toyota.ConnectTimeOut = 3000; // 连接3秒超时
			OperateResult connect = await toyota.ConnectServerAsync( );
			if (connect.IsSuccess)
			{
				MessageBox.Show( HslCommunication.StringResources.Language.ConnectedSuccess );
				button2.Enabled = true;
				button1.Enabled = false;
				panel2.Enabled = true;

				// 设置子控件的读取能力
				userControlReadWriteDevice1.SetReadWriteNet( toyota, "D100", true );
				// 设置批量读取
				userControlReadWriteDevice1.BatchRead.SetReadWriteNet( toyota, "D100", string.Empty );
				//userControlReadWriteDevice1.BatchRead.SetReadRandom( melsec_net.ReadRandom, "D100;W100;D500" );
				//userControlReadWriteDevice1.BatchRead.SetReadWordRandom( melsec_net.ReadRandom, "D100;W100;D500" );
				// 设置报文读取
				userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => toyota.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );

			}
			else
			{
				MessageBox.Show( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message );
				button1.Enabled = true;
			}
		}


		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			toyota.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}

		#endregion


		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlIpAddress, textBox_ip.Text );
			element.SetAttributeValue( DemoDeviceList.XmlPort, textBox_port.Text );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			textBox_ip.Text = element.Attribute( DemoDeviceList.XmlIpAddress ).Value;
			textBox_port.Text = element.Attribute( DemoDeviceList.XmlPort ).Value;

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}

	}
}