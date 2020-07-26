﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UAssetAPI;

namespace UAssetGUI
{
    public partial class Form1 : Form
    {
        public TableHandler tableEditor;
        public ByteViewer byteView1;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.Visible = true;

            // Extra data viewer
            byteView1 = new ByteViewer
            {
                AutoScroll = true,
                AutoSize = true,
                Visible = false
            };
            Controls.Add(byteView1);

            // Enable double buffering to look nicer
            if (!SystemInformation.TerminalServerSession)
            {
                Type ourGridType = dataGridView1.GetType();
                PropertyInfo pi = ourGridType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dataGridView1, true, null);
            }

            // Auto resizing
            SizeChanged += frm_sizeChanged;

            // Command line parameters
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LoadFileAt(args[1]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void LoadFileAt(string filePath)
        {
            dataGridView1.Visible = true;
            byteView1.Visible = false;

            try
            {
                currentSavingPath = filePath;
                SetUnsavedChanges(false);

                tableEditor = new TableHandler(dataGridView1, new AssetWriter(filePath, true, true, null, null), listView1)
                {
                    mode = TableHandlerMode.HeaderList
                };

                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;

                tableEditor.FillOutTree();
                tableEditor.Load();
            }
            catch (IOException)
            {
                currentSavingPath = "";
                SetUnsavedChanges(false);
                tableEditor = null;
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;

                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();
                dataGridView1.BackgroundColor = Color.FromArgb(211, 211, 211);
            }
        }

        public bool existsUnsavedChanges = false;
        public void SetUnsavedChanges(bool flag)
        {
            if (string.IsNullOrEmpty(currentSavingPath)) return;
            existsUnsavedChanges = flag;
            if (existsUnsavedChanges)
            {
                dataGridView1.Parent.Text = "UAssetGUI - *" + currentSavingPath;
            }
            else
            {
                dataGridView1.Parent.Text = "UAssetGUI - " + currentSavingPath;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "UAssets (*.uasset)|*.uasset|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFileAt(openFileDialog.FileName);
                }
            }
        }

        private string currentSavingPath = "";

        private void ForceSave(string path)
        {
            if (tableEditor != null && !string.IsNullOrEmpty(currentSavingPath))
            {
                try
                {
                    tableEditor.asset.Write(path);
                    SetUnsavedChanges(false);
                    tableEditor.Load();
                }
                catch
                {
                    MessageBox.Show("Failed to save!", "Uh oh!");
                }
            }
            else
            {
                MessageBox.Show("Failed to save!", "Uh oh!");
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "UAssets (*.uasset)|*.uasset|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                DialogResult res = dialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    currentSavingPath = dialog.FileName;
                    ForceSave(currentSavingPath);
                }
                else if (res != DialogResult.Cancel)
                {
                    MessageBox.Show("Failed to save!", "Uh oh!");
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ForceSave(currentSavingPath);
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.RestoreTreeState(true);
            }
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.RestoreTreeState(false);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataObject dataObj = dataGridView1.GetClipboardContent();
                if (dataObj != null)
                {
                    Clipboard.SetDataObject(dataObj, true);
                }
            }
            catch
            {

            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                tableEditor.readyToSave = false;
                string s = Clipboard.GetText();
                string[] lines = s.Split('\n');
                int iRow = dataGridView1.CurrentCell.RowIndex;
                int iCol = dataGridView1.CurrentCell.ColumnIndex;
                DataGridViewCell oCell;
                foreach (string line in lines)
                {
                    if (iRow < dataGridView1.RowCount && line.Length > 0)
                    {
                        string[] sCells = line.Split('\t');
                        for (int i = 0; i < sCells.GetLength(0); ++i)
                        {
                            if (iCol + i < this.dataGridView1.ColumnCount)
                            {
                                oCell = dataGridView1[iCol + i, iRow];
                                if (!oCell.ReadOnly)
                                {
                                    if (oCell == null || oCell.Value == null || oCell.Value.ToString() != sCells[i])
                                    {
                                        oCell.Value = Convert.ChangeType(sCells[i], oCell.ValueType);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        iRow++;
                    }
                    else
                    {
                        break;
                    }
                }
                tableEditor.readyToSave = true;
                tableEditor.Save(true);
            }
            catch (Exception ex)
            {
                tableEditor.readyToSave = true;
                Debug.WriteLine(ex);
                MessageBox.Show("Failed to paste data", "Uh oh!");
            }
        }

        private void dataGridEditCell(object sender, EventArgs e)
        {
            if (tableEditor != null && tableEditor.readyToSave)
            {
                tableEditor.Save(false);
            }
        }

        private void dataGridClickCell(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Style != null && dataGridView1.CurrentCell.Style.Font != null && dataGridView1.CurrentCell.Style.Font.Underline == true)
            {
                switch(dataGridView1.CurrentCell.Value)
                {
                    case "Jump":
                        if (dataGridView1.CurrentCell.ColumnIndex == 3)
                        {
                            DataGridViewCell previousCell = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[dataGridView1.CurrentCell.ColumnIndex - 1];
                            if (previousCell == null || previousCell.Value == null) return;

                            int jumpingTo = -1;
                            if (previousCell.Value is string) int.TryParse((string)previousCell.Value, out jumpingTo);
                            if (previousCell.Value is int) jumpingTo = (int)previousCell.Value;
                            if (jumpingTo < 0) return;

                            TreeNode topSelectingNode = listView1.Nodes[listView1.Nodes.Count - 1];
                            if (topSelectingNode.Nodes.Count > (jumpingTo - 1))
                            {
                                topSelectingNode = topSelectingNode.Nodes[jumpingTo - 1];
                                if (topSelectingNode.Nodes.Count > 0)
                                {
                                    topSelectingNode = topSelectingNode.Nodes[0];
                                }
                            }
                            listView1.SelectedNode = topSelectingNode;
                        }
                        break;
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            string selectedNodeText = e.Node.Text;
            if (tableEditor != null)
            {
                tableEditor.mode = TableHandlerMode.CategoryData;
                switch (selectedNodeText)
                {
                    case "Header List":
                        tableEditor.mode = TableHandlerMode.HeaderList;
                        break;
                    case "Linked Sectors":
                        tableEditor.mode = TableHandlerMode.LinkedSectors;
                        break;
                    case "Section Information":
                        tableEditor.mode = TableHandlerMode.CategoryInformation;
                        break;
                    /*case 3:
                        tableEditor.mode = TableHandlerMode.CategoryInts;
                        break;
                    case 4:
                        tableEditor.mode = TableHandlerMode.CategoryStrings;
                        break;*/
                }
                tableEditor.Load();
            }
        }

        public void ForceResize()
        {
            float widthAmount = 0.6f;
            dataGridView1.Size = new Size((int)(this.Size.Width * widthAmount), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            dataGridView1.Location = new Point(this.Size.Width - dataGridView1.Size.Width - this.menuStrip1.Size.Height, this.menuStrip1.Size.Height);
            if (byteView1 != null)
            {
                byteView1.Size = dataGridView1.Size;
                byteView1.Location = dataGridView1.Location;
                byteView1.Refresh();
            }

            listView1.Size = new Size((int)(this.Size.Width * (1 - widthAmount)) - (this.menuStrip1.Size.Height * 2), this.Size.Height - (this.menuStrip1.Size.Height * 3));
            listView1.Location = new Point(this.menuStrip1.Size.Height / 2, this.menuStrip1.Size.Height);
        }

        private void frm_sizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.Save(true);
            }
        }

        private void refreshFullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableEditor != null)
            {
                tableEditor.Save(true);
                tableEditor.FillOutTree();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/atenfyr/uassetapi");
        }
    }
}
