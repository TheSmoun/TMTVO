﻿#pragma checksum "..\..\Controls.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2095D0D75227AC4D5FFB49006F37F3D7"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34014
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace TMTVO {
    
    
    /// <summary>
    /// Controls
    /// </summary>
    public partial class Controls : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle Running;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StartStopButton;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid InnerGrid;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SessionTimer;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid RaceButtons;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid NormalButtons;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid LapTimerTest;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ToggleLapTimerLeft;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SectorCompleteTest;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\Controls.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CrossedLine;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TMTVO;component/controls.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Controls.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 4 "..\..\Controls.xaml"
            ((TMTVO.Controls)(target)).Closed += new System.EventHandler(this.Window_Closed);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Running = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 3:
            this.StartStopButton = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\Controls.xaml"
            this.StartStopButton.Click += new System.Windows.RoutedEventHandler(this.StartStopButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.InnerGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.SessionTimer = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\Controls.xaml"
            this.SessionTimer.Click += new System.Windows.RoutedEventHandler(this.SessionTimer_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.RaceButtons = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.NormalButtons = ((System.Windows.Controls.Grid)(target));
            return;
            case 8:
            this.LapTimerTest = ((System.Windows.Controls.Grid)(target));
            return;
            case 9:
            this.ToggleLapTimerLeft = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\Controls.xaml"
            this.ToggleLapTimerLeft.Click += new System.Windows.RoutedEventHandler(this.ToggleLapTimerLeft_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.SectorCompleteTest = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\Controls.xaml"
            this.SectorCompleteTest.Click += new System.Windows.RoutedEventHandler(this.SectorCompleteTest_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.CrossedLine = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\Controls.xaml"
            this.CrossedLine.Click += new System.Windows.RoutedEventHandler(this.CrossedLine_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

