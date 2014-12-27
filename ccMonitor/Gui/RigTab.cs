﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ccMonitor.Api;
using Newtonsoft.Json;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace ccMonitor.Gui
{
    public partial class RigTab : UserControl
    {
        private RigController.RigInfo Rig { get; set; }

        private List<string> _apiCommands;
        private int _apiCommandsIndex;
        private string _stringBeforeUpOrDown;

        public RigTab(RigController.RigInfo rig)
        {
            Rig = rig;
            _apiCommands = new List<string>();
            InitializeComponent();
        }

        public void UpdateGui()
        {
            // Adds missing gpu tabs
            foreach (GpuLogger gpu in Rig.GpuLogs)
            {
                bool found = false;
                foreach (TabPage tabPage in tbcRig.TabPages)
                {
                    if (tabPage != tabRigOverview && tabPage != tabRigApiConsole
                        && tabPage.Text == gpu.Info.ToString())
                    {
                        found = true;
                        foreach (GpuTab gpuTab in tabPage.Controls)
                        {
                            gpuTab.UpdateGui();
                        }
                        break;
                    }
                }

                if (!found)
                {
                    TabPage tabPage = new TabPage(gpu.Info.ToString());
                    GpuTab gpuGpuTab = new GpuTab(gpu) { Dock = DockStyle.Fill };
                    gpuGpuTab.UpdateGui();
                    tabPage.Controls.Add(gpuGpuTab);
                    tbcRig.TabPages.Add(tabPage);
                }
            }

            // Removes deleted gpus
            List<TabPage> sentencedTabPages = new List<TabPage>();
            foreach (TabPage tabPage in tbcRig.TabPages)
            {
                if (tabPage != tabRigOverview && tabPage != tabRigApiConsole)
                {
                    bool found = false;
                    foreach (GpuLogger gpu in Rig.GpuLogs)
                    {
                        if (tabPage.Text == gpu.Info.ToString())
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        sentencedTabPages.Add(tabPage);
                    }
                }
            }

            foreach (TabPage tabPage in sentencedTabPages)
            {
                tbcRig.TabPages.Remove(tabPage);
            }
        }

        private void txtApiConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string[] splitStrings = txtApiConsole.Text.Split('>');
                string trimmed = splitStrings[splitStrings.Length - 1].Trim();
                _apiCommands.Insert(0, trimmed);

                Dictionary<string, string>[] request = PruvotApi.Request(Rig.IpAddress, Rig.Port, trimmed);
                if (request != null)
                {
                    txtApiConsole.AppendText(Environment.NewLine + JsonConvert.SerializeObject(request, Formatting.Indented)
                                                + Environment.NewLine + " >  ");
                    e.SuppressKeyPress = true;
                    _apiCommandsIndex = 0;
                }
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                if (_stringBeforeUpOrDown == string.Empty)
                    _stringBeforeUpOrDown = txtApiConsole.Lines[txtApiConsole.Lines.Length - 1];
                StringBuilder sb = new StringBuilder(txtApiConsole.TextLength);
                for (int index = 0; index < txtApiConsole.Lines.Length - 1; index++)
                {
                    sb.AppendLine(txtApiConsole.Lines[index]);
                }

                if (_apiCommands.Count > 0)
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        if (_apiCommandsIndex >= _apiCommands.Count)
                        {
                            _apiCommandsIndex = _apiCommands.Count - 1;
                        }

                        sb.AppendLine(" >  " + _apiCommands[_apiCommandsIndex]);
                    }
                    else
                    {
                        _apiCommandsIndex -= 2;
                        if (_apiCommandsIndex < 0)
                        {
                            _apiCommandsIndex = -1;
                            sb.AppendLine(_stringBeforeUpOrDown);
                        }
                        else
                        {
                            sb.AppendLine(" >  " + _apiCommands[_apiCommandsIndex]);
                        }
                    }

                    _apiCommandsIndex++;


                }
                else
                {
                    sb.AppendLine(_stringBeforeUpOrDown);
                }

                txtApiConsole.Text = sb.ToString(0, sb.Length - 2);
                txtApiConsole.SelectionStart = txtApiConsole.TextLength;
                txtApiConsole.ScrollToCaret();

                e.SuppressKeyPress = true;
            }
            else
            {
                _stringBeforeUpOrDown = string.Empty;
            }
        }
    }
}
