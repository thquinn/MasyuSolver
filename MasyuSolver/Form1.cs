using MasyuSolver.Subforms;
using System;
using System.Windows.Forms;

namespace MasyuSolver {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            masyuPanel.textBox = textBox1;
        }

        private void Menu_New(object sender, EventArgs e) {
            NewDialog dlg = new NewDialog();
            if (dlg.ShowDialog() == DialogResult.OK) {
                masyuPanel.New(dlg.puzzleWidth, dlg.puzzleHeight);
            }
        }
        private void Menu_Solve(object sender, EventArgs e) {
            masyuPanel.Solve();
        }
    }
}
