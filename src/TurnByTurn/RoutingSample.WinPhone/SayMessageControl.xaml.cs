using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RoutingSample
{
	public sealed partial class SayMessageControl : UserControl
	{
		public SayMessageControl()
		{
			this.InitializeComponent();
		}


		/// <summary>
		/// Say message when this reaches '0 + offset'
		/// </summary>
		public TimeSpan TimeToMessage
		{
			get { return (TimeSpan)GetValue(TimeToMessageProperty); }
			set { SetValue(TimeToMessageProperty, value); }
		}

		public static readonly DependencyProperty TimeToMessageProperty =
			DependencyProperty.Register("TimeToMessage", typeof(TimeSpan), typeof(SayMessageControl), new PropertyMetadata(TimeSpan.Zero, OnTimeToMessagePropertyChanged));

		private static void OnTimeToMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (d as SayMessageControl);
			ctrl.timeToMessage = (TimeSpan)e.NewValue;
			ctrl.SayMessage(ctrl.Message);
		}

		/// <summary>
		/// Say message when TimeToMessage is 'Offset' seconds from 0 
		/// </summary>
		public double Offset
		{
			get { return (double)GetValue(OffsetProperty); }
			set { SetValue(OffsetProperty, value); }
		}

		public static readonly DependencyProperty OffsetProperty =
			DependencyProperty.Register("Offset", typeof(double), typeof(SayMessageControl), new PropertyMetadata(20d));

		private TimeSpan timeToMessage = TimeSpan.MaxValue;	

		public string Message
		{
			get { return (string)GetValue(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register("Message", typeof(string), typeof(SayMessageControl), new PropertyMetadata("", OnMessagePropertyChanged));

		private static void OnMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (d as SayMessageControl);
			if (ctrl.Offset > 0)
				ctrl.timeToMessage = TimeSpan.MaxValue; //prevent message from being read out until time updates next time
			string message = e.NewValue as string;
			ctrl.SayMessage(message);
		}

		private string lastMessage = null;

		private async void SayMessage(string message)
		{
			if (!string.IsNullOrWhiteSpace(message)
				 && timeToMessage.TotalSeconds <= Offset)
			{
				if(lastMessage == message)
					return;
				try
				{
					using (var speech = new Windows.Media.SpeechSynthesis.SpeechSynthesizer())
					{
						lastMessage = message;
						var voiceStream = await speech.SynthesizeTextToStreamAsync(ReplaceAbbreviations(message));
						player.SetSource(voiceStream, voiceStream.ContentType);
						player.Play();
					}
				}
				catch { }
			}
		}
		private static string ReplaceAbbreviations(string message)
		{
			var words = message.Split(new char[] { ' ' });
			string[] words2 = new string[words.Length];
			for (int i = 0; i < words.Length; i++)
			{
				var word = words[i];
				switch(word)
				{
					case "W": word = "west"; break;
					case "N": word = "west"; break;
					case "S": word = "south"; break;
					case "E": word = "east"; break;
					case "St": word = "Street"; break;
					case "Ave": word = "Avenue"; break;
					case "Pl": word = "Place"; break;
					case "Dr": word = "Drive"; break;
					default: break;
				}
				words2[i] = word;
			}

			return string.Join(" ", words2);
		}
	}
}
