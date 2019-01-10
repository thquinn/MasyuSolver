﻿using MasyuSolver.Subforms;
using System;
using System.Windows.Forms;

namespace MasyuSolver {
    public partial class MasyuForm : Form {
        public int solveDepth;

        public MasyuForm() {
            InitializeComponent();
            masyuPanel.form = this;
            solveDepth = 2;
        }

        public void UpdateValidity(bool valid) {
            if (valid) {
                textBox1.Text = "VALID";
                textBox1.BackColor = System.Drawing.Color.White;
                textBox1.ForeColor = System.Drawing.Color.Black;
            } else {
                textBox1.Text = "INVALID";
                textBox1.BackColor = System.Drawing.Color.Red;
                textBox1.ForeColor = System.Drawing.Color.White;
            }
        }

        private void Menu_New(object sender, EventArgs e) {
            NewDialog dlg = new NewDialog();
            if (dlg.ShowDialog() == DialogResult.OK) {
                masyuPanel.New(dlg.puzzleWidth, dlg.puzzleHeight);
            }
        }
        private void Menu_SolveDepth(object sender, EventArgs e) {
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.Checked) {
                return;
            }
            MenuItem[] solveDepthItems = new MenuItem[] { menuItem3, menuItem6, menuItem7 };
            for (int i = 0; i < solveDepthItems.Length; i++) {
                MenuItem solveDepthItem = solveDepthItems[i];
                if (menuItem == solveDepthItem) {
                    solveDepthItem.Checked = true;
                    solveDepth = i;
                } else {
                    solveDepthItem.Checked = false;
                }
            }
            masyuPanel.Solve();
        }
    }
}
