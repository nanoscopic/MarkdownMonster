﻿#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 05/15/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.IO;
using System.Windows.Controls;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace SnagItAddin
{

    public class SnagitAddin : MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            // Add a menu item
            var menuItem = new AddInMenuItem()
            {                
                Caption = "SnagIt Screen Capture",
                FontawesomeIcon= FontAwesomeIcon.Camera,

                // a unique command id that is tied to the menuitem
                EditorCommand = "snagit",
            };
            menuItem.Execute = new Action<object>(SnagitMenu_Execute);
            menuItem.ExecuteConfiguration = new Action<object>(SnagitConfigurationMenu_Execute);
            menuItem.CanExecute = new Func<object,bool>(SnagitConfigurationMenu_CanExecute);
            
            this.MenuItems.Add(menuItem);
        }

        public void SnagitMenu_Execute(object sender)
        {
            var config = ScreenCaptureConfiguration.Current;

            if (config.AlwaysShowCaptureOptions)
            {
                var form = new ScreenCaptureConfigurationForm()
                {
                    Owner = Model.Window,
                    IsPreCaptureMode = true
                };

                var result = form.ShowDialog();
                if (result == null || !result.Value)
                    return;
            }


            SnagItAutomation SnagIt = SnagItAutomation.Create();

            
            var editor = Model.Window.GetActiveMarkdownEditor();

            SnagIt.CapturePath = editor?.MarkdownDocument.Filename;
            if (!string.IsNullOrEmpty(SnagIt.CapturePath))
                SnagIt.CapturePath = Path.GetDirectoryName(SnagIt.CapturePath);

                    
            string capturedFile = SnagIt.CaptureImageToFile();
            if (string.IsNullOrEmpty(capturedFile) || !File.Exists(capturedFile))
                return;

            capturedFile = FileUtils.GetRelativePath(capturedFile, SnagIt.CapturePath);
            string relPath = capturedFile.Replace("\\", "/");
            if (relPath.StartsWith(".."))
                relPath = capturedFile;

            string replaceText = "![](" +  relPath + ")";
            
            
            // Push the new text into the Editor's Selection
            this.SetSelection(replaceText);
        }

        public void SnagitConfigurationMenu_Execute(object sender)
        {
            var configForm = new ScreenCaptureConfigurationForm()
            {
                Owner = this.Model.Window
            };            
            configForm.Show();
        }

        public bool SnagitConfigurationMenu_CanExecute(object sender)
        {            
            if (!SnagItAutomation.IsInstalled)
            {
                var button = sender as Button;
                button.ToolTip = "SnagIt isn't installed. Currently only SnagIt based captures are supported.";
                button.IsEnabled = false;    
                return false;
            }

            return true;
        }
    }
}
