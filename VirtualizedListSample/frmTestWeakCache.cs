using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace VirtualizedListSample
{
    public partial class frmTestWeakCache : Form
    {
        public frmTestWeakCache()
        {
            InitializeComponent();

            FormClosing += frmTestWeakCache_FormClosing;
        }

        void frmTestWeakCache_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isThread1Running = false;
            _isThread2Running = false;

            Thread.Sleep(1000);         // give it a second..

            cache.Dispose();            // no longer needed
        }

        private class TestClass
        {
            public TestClass(int val)
            {
                Value = val;
            }

            public int Value
            {
                get;
                private set;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        DNQ.VirtualizedItemsSource.WeakCache<int, TestClass> cache = DNQ.VirtualizedItemsSource.WeakCache.Create<int, TestClass>(TimeSpan.FromMinutes(9));
        DNQ.VirtualizedItemsSource.WeakCache<int, TestClass> cache1 = DNQ.VirtualizedItemsSource.WeakCache.Create<int, TestClass>(TimeSpan.FromMinutes(3));
        DNQ.VirtualizedItemsSource.WeakCache<int, TestClass> cache2 = DNQ.VirtualizedItemsSource.WeakCache.Create<int, TestClass>(TimeSpan.FromMinutes(6));

        private bool _isThread1Running = false;
        private bool _isThread2Running = false;

        private void btnToggleThread1_Click(object sender, EventArgs e)
        {
            if (_isThread1Running)
            {
                _isThread1Running = false;
                btnToggleThread1.Text = "Start Thread 1";
            }
            else
            {
                _isThread1Running = true;
                btnToggleThread1.Text = "Stop Thread 1";
                Thread t = new Thread(RunThread1);
                t.Name = "Thread1";
                t.IsBackground = true;
                t.Start();                
            }
        }

        private void btnToggleThread2_Click(object sender, EventArgs e)
        {
            if (_isThread2Running)
            {
                _isThread2Running = false;
                btnToggleThread1.Text = "Start Thread 2";
            }
            else
            {
                _isThread2Running = true;
                btnToggleThread2.Text = "Stop Thread 2";
                Thread t = new Thread(RunThread2);
                t.IsBackground = true;
                t.Name = "Thread2";
                t.Start();
            }
        }

        private void RunThread1(object state)
        {
            Random rnd = new Random();

            try
            {
                while (true)
                {
                    int idx = rnd.Next(100);
                    if (!cache.Has(idx))
                    {
                        cache.Put(idx, new TestClass(idx));
                    }
                    else
                    {
                        var t = cache.Get(idx);
                        if (t.Value != idx)
                        {
                            throw new InvalidOperationException("Cache corruption detected on Thread 1");
                        }
                    }

                    Thread.Sleep(30);
                    if (!_isThread1Running)
                        break;
                }
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
                _isThread1Running = false;

                this.Invoke((Action)delegate
                {
                    btnToggleThread1.Text = "Start Thread 1";
                });
            }
        }

        private void RunThread2(object state)
        {
            Random rnd = new Random();

            while (true)
            {
                int idx = rnd.Next(100);                
                cache.Put(idx, new TestClass(idx));

                Thread.Sleep(20);
                if (!_isThread2Running)
                    break;

                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    try
                    {
                        this.Invoke((Action)delegate
                        {
                            lblCacheInfo.Text = string.Format("{0} items in cache ({1} valid)", cache.Count, cache.GetValidCount());
                        });
                    }
                    catch { }
                }
            }
        }
       
        private void btnPurge_Click(object sender, EventArgs e)
        {
            var result = cache.Purge();
            if (result == DNQ.VirtualizedItemsSource.PurgeResult.ErrorTimedout)
            {
                MessageBox.Show("Purge timedout. It was blocked..");
            }
            else if (result == DNQ.VirtualizedItemsSource.PurgeResult.NotNeeded)
            {
                MessageBox.Show("Purge complete: no items needed purging..");
            }
            else
            {
                MessageBox.Show("Purge complete: " + result.NumberOrItemsPurged.ToString() + " items purged..");
            }
        }

        private void btnForceGC_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}
