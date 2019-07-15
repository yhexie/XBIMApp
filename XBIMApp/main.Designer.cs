namespace XBIMApp
{
    partial class main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.创建ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TspCreateIFC = new System.Windows.Forms.ToolStripMenuItem();
            this.TspMergeIFC = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TspCreateIFC2 = new System.Windows.Forms.ToolStripMenuItem();
            this.tspCreateSlabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.创建ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 创建ToolStripMenuItem
            // 
            this.创建ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TspCreateIFC,
            this.TspCreateIFC2,
            this.tspCreateSlabToolStripMenuItem,
            this.toolStripSeparator1,
            this.TspMergeIFC});
            this.创建ToolStripMenuItem.Name = "创建ToolStripMenuItem";
            this.创建ToolStripMenuItem.Size = new System.Drawing.Size(103, 24);
            this.创建ToolStripMenuItem.Text = "创建IFC模型";
            // 
            // TspCreateIFC
            // 
            this.TspCreateIFC.Name = "TspCreateIFC";
            this.TspCreateIFC.Size = new System.Drawing.Size(271, 26);
            this.TspCreateIFC.Text = "根据门窗创建IFC";
            this.TspCreateIFC.Click += new System.EventHandler(this.TspCreateIFC_Click);
            // 
            // TspMergeIFC
            // 
            this.TspMergeIFC.Name = "TspMergeIFC";
            this.TspMergeIFC.Size = new System.Drawing.Size(271, 26);
            this.TspMergeIFC.Text = "合并2个IFC";
            this.TspMergeIFC.Click += new System.EventHandler(this.TspMergeIFC_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(750, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::XBIMApp.Properties.Resources._3D_printer_71_441860465116px_1166648_easyicon_net;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::XBIMApp.Properties.Resources._3D_printer_46_663573085847px_1166645_easyicon_net;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(24, 24);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(268, 6);
            // 
            // TspCreateIFC2
            // 
            this.TspCreateIFC2.Name = "TspCreateIFC2";
            this.TspCreateIFC2.Size = new System.Drawing.Size(271, 26);
            this.TspCreateIFC2.Text = "根据门窗创建IFC（带字段）";
            this.TspCreateIFC2.Click += new System.EventHandler(this.TspCreateIFC2_Click);
            // 
            // tspCreateSlabToolStripMenuItem
            // 
            this.tspCreateSlabToolStripMenuItem.Name = "tspCreateSlabToolStripMenuItem";
            this.tspCreateSlabToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.tspCreateSlabToolStripMenuItem.Text = "创建楼板";
            this.tspCreateSlabToolStripMenuItem.Click += new System.EventHandler(this.tspCreateSlabToolStripMenuItem_Click);
            // 
            // main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 427);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "main";
            this.Text = "IFC模型处理工具箱";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 创建ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TspCreateIFC;
        private System.Windows.Forms.ToolStripMenuItem TspMergeIFC;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripMenuItem TspCreateIFC2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tspCreateSlabToolStripMenuItem;
    }
}