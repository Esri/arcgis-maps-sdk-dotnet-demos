using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExternalNmeaGPS.Controls
{
	/// <summary>
	/// Interaction logic for NmeaRawMessagesWindow.xaml
	/// </summary>
	public partial class NmeaRawMessagesWindow : Window
	{
		private Queue<string> messages = new Queue<string>(101);
		
		public NmeaRawMessagesWindow()
		{
			InitializeComponent();
		}
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
			base.OnClosing(e);
		}

		public void AddMessage(NmeaParser.Nmea.NmeaMessage message)
		{
			messages.Enqueue(message.MessageType + ": " + message.ToString());
			if (messages.Count > 100) messages.Dequeue(); //Keep message queue at 100
			output.Text = string.Join("\n", messages.ToArray());
			output.Select(output.Text.Length - 1, 0); //scroll to bottom
		}
	}
}
