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
    public partial class frmMain 
        : Form
    {
        List<DemoItem> _sourceDataSet = new List<DemoItem>();

        private void GenerateRandomDataSet(int capacity)
        {
            string[] detailedText = new string[] { "These are details about item {0}", "Some detailed information about item number {0}", "Item {0}'s detailed descritption" };
            Random rnd = new Random();

            for (int i = 0; i < capacity; i++)
            {
                _sourceDataSet.Add(new DemoItem(string.Format("Item {0}", i), string.Format(detailedText[rnd.Next(3)], i)));
            }
        }

        private DNQ.VirtualizedItemsSource.VirtualizedItemSource<DemoItem> _virtualizedItemsSource;
        private DNQ.VirtualizedItemsSource.WeakCache<int, ListViewItem> listItemsCache;

        public frmMain()
        {
            InitializeComponent();

            // make 5000 items to play with
            GenerateRandomDataSet(50000);

            Load += frmMain_Load;
        }

        void frmMain_Load(object sender, EventArgs e)
        {
            _virtualizedItemsSource = new DNQ.VirtualizedItemsSource.VirtualizedItemSource<DemoItem>(500, CountItems, RetrieveItems, LogVirtualizedListMessages);
            _virtualizedItemsSource.SourceReinitialized += VirtualizedAuditEventsSource_SourceReinitialized;
            _virtualizedItemsSource.NotifyRepositionRequired += VirtualizedAuditEventsSource_NotifyRepositionRequired;
            _virtualizedItemsSource.SourceRepositioned += VirtualizedAuditEventsSource_SourceRepositioned;
            _virtualizedItemsSource.NotifyErrorOccured += VirtualizedAuditEventsSource_NotifyErrorOccured;

            listItemsCache = DNQ.VirtualizedItemsSource.WeakCache.Create<int, ListViewItem>();

            lstItems.RetrieveVirtualItem += lstItems_RetrieveVirtualItem;
            lstItems.DoubleClick += lstItems_DoubleClick;

            DisableUI();

            ThreadPool.QueueUserWorkItem((WaitCallback)delegate
            {
                Thread.Sleep(200);

                Application.DoEvents();

                try
                {
                    _virtualizedItemsSource.ReinitializeSource(-1);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error initializing virtual events source. {0}", exc.Message);
                    this.BeginInvoke((Action)delegate
                    {
                        lstItems.VirtualListSize = 0;
                    });

                    ShowErrorNotification(string.Format("Error loading data: {0}", exc.Message));
                }
            });
        }

        private int CountItems()
        {
            return _sourceDataSet.Count;
        }

        private volatile int _delay = 50;
        private IEnumerable<DemoItem> RetrieveItems(int offset, int count, System.Threading.CancellationToken cancellationToken)
        {            
            // simulate a long-running operation such as querying a database
            var lst = new List<DemoItem>();

            Thread.Sleep(_delay);

            for(int i = 0; i < count; i++)
            {
                lst.Add(_sourceDataSet[offset + i]);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            // and retrieve the requested list of items for the virtualized items source
            return lst;
        }

        private void LogVirtualizedListMessages(int level, string message)
        {
            if (level < 3)
            {
                Console.WriteLine("AuditTrailsReportDialog.VirtualizedList [ERROR]: {0}", message);
            }
            else
            {
                Console.WriteLine("AuditTrailsReportDialog.VirtualizedList [INFO]: {0}", message);
            }
        }

        private void VirtualizedAuditEventsSource_NotifyErrorOccured(object sender, DNQ.VirtualizedItemsSource.OperationErrorEventArgs e)
        {
            string errMessage = e.StoredException != null ? e.StoredException.Message : "UNKNOWN ERROR";
            Console.WriteLine("AuditTrailsReportDialog.VirtualizedList Error in virtualized audit event source: {0}", errMessage);

            if (this.IsHandleCreated)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((Action)delegate
                    {
                        lstItems.VirtualListSize = 0;
                    });
                }
            }

            ShowErrorNotification(string.Format("Error loading data: {0}", errMessage));

            listItemsCache.Clear();
            e.ContinueExecution = false;
        }

        private void VirtualizedAuditEventsSource_SourceRepositioned(object sender, DNQ.VirtualizedItemsSource.RepositionCompleteEventArgs e)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((EventHandler<DNQ.VirtualizedItemsSource.RepositionCompleteEventArgs>)VirtualizedAuditEventsSource_SourceRepositioned, sender, e);
                return;
            }

            if (e.RefreshRequired)
                lstItems.Refresh();

            EnableUI();
        }

        private void VirtualizedAuditEventsSource_NotifyRepositionRequired(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((EventHandler<EventArgs>)VirtualizedAuditEventsSource_NotifyRepositionRequired, sender, e);
                return;
            }

            DisableUI();
        }

        private void VirtualizedAuditEventsSource_SourceReinitialized(object sender, DNQ.VirtualizedItemsSource.SourceReinitializedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((EventHandler<DNQ.VirtualizedItemsSource.SourceReinitializedEventArgs>)VirtualizedAuditEventsSource_SourceReinitialized, sender, e);
                return;
            }

            if (e.StoredException == null)
            {
                tsStatusLabel.Text = string.Format("{0} records loaded in {1:0.000} seconds", _virtualizedItemsSource.TotalCount, Math.Round(e.Duration.TotalSeconds, 3));
                lstItems.VirtualListSize = e.Size;
            }
            else
            {
                lstItems.VirtualListSize = 0;
                tsStatusLabel.Text = "Error loading records..";
            }

            EnableUI();
        }

        private ListViewItem MakeListItem(DemoItem demoItem)
        {
            var lvi = new ListViewItem();
            if (demoItem != null)
            {
                lvi.Text = demoItem.Name;
                
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = demoItem.Details });                                
            }
            else
            {
                lvi.SubItems.Add("");
            }

            return lvi;
        }

        void lstItems_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var item = listItemsCache.Get(e.ItemIndex);
            if (item == null)
            {
                DemoItem demoItem = null;
                if (_virtualizedItemsSource.GetItem(e.ItemIndex, out demoItem))
                {
                    item = MakeListItem(demoItem);

                    listItemsCache.Put(e.ItemIndex, item);
                }
                else
                {
                    // it probably means that the list size has changed..
                    lstItems.VirtualListSize = _virtualizedItemsSource.TotalCount;
                    e.Item = MakeListItem(null);
                    return;
                }
            }
            e.Item = item;
        }

        private void lstItems_DoubleClick(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            if (lstItems.SelectedIndices.Count > 0)
            {
                selectedIndex = lstItems.SelectedIndices[0];
            }

            if (selectedIndex != -1)
            {
                DemoItem selectedItem = null;
                _virtualizedItemsSource.GetItem(selectedIndex, out selectedItem);

                if (selectedItem != null)
                {
                    MessageBox.Show("You clicked on item:\r\n\r\n\t" + selectedItem.Name, "Item Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void EnableUI()
        {
        }

        private void DisableUI()
        {
        }

        private void lblListErrorNotification_Click(object sender, EventArgs e)
        {
            pnlListErrorIndicator.Visible = false;
        }

        private void ShowErrorNotification(string message)
        {
            if (this.IsHandleCreated)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((Action)delegate
                    {
                        pnlListErrorIndicator.Visible = true;
                        lblListErrorNotification.Text = message;
                    });
                }
                else
                {
                    pnlListErrorIndicator.Visible = true;
                    lblListErrorNotification.Text = message;
                }
            }
        }

        private void btnToggleListMode_Click(object sender, EventArgs e)
        {
            if (lstItems.View == View.Details)
            {
                lstItems.View = View.LargeIcon;
            }else if(lstItems.View == View.LargeIcon)
            {
                lstItems.View = View.SmallIcon;
            }else
            {
                lstItems.View = View.Details;
            }
        }

        private void numDelay_ValueChanged(object sender, EventArgs e)
        {
            _delay = (int)numDelay.Value;
        }
    }
}
