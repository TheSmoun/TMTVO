﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMTVO.Widget
{
	/// <summary>
	/// Interaktionslogik für PreviousGaps.xaml
	/// </summary>
	public partial class PreviousGaps : UserControl
	{
        private readonly int MAX_WIDTH = 320;

		public PreviousGaps()
		{
			this.InitializeComponent();
		}
	}
}