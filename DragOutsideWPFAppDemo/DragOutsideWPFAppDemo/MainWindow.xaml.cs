namespace DragOutsideWPFAppDemo
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Delegate that will handles the routed events that enables a drag-and-drop operation to be canceled by the drag source
        private readonly QueryContinueDragEventHandler queryhandler;

        //Mouse coordinate starting  point
        private Point _startPoint;

        public MainWindow()
        {
            this.InitializeComponent();

            //creating handler for Drag and Drop 
            this.queryhandler = this.DragSourceQueryContinueDrag;
        }

        /// <summary>
        ///     Left mouse down route event handler
        /// </summary>
        /// <param name="sender">Object that owns event</param>
        /// <param name="e">Mouse button data information /param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //  Captured the starting point, 
            Debug.WriteLine(" window left button down");
            this._startPoint = e.GetPosition(null);
        }

        /// <summary>
        ///     Mouse move route event handler
        /// </summary>
        /// <param name="sender">Object that owns route eventt</param>
        /// <param name="e" M>ouse move data information</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //Calculated distance between starting point and the current mouse position. 
            var mpos = e.GetPosition(null);
            var diff = this._startPoint - mpos;

            //If the distance between is big enough  start DragDrop
            if (e.LeftButton == MouseButtonState.Pressed
                && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                //hooking on Mouse Up
                InterceptMouse.m_hookID = InterceptMouse.SetHook(InterceptMouse.m_proc);

                //Attach drag and drop route handler 
                this.QueryContinueDrag += this.queryhandler;
               
                // Create data information to de dropped 
                DataObject data = new DataObject();


                // Add a DataObjectInformation object using a custom format name.
                data.SetData("DataObjectInformation",
                    new DataObjectInformation("DataObjectInformation"));


                // place object in clopboard
                Clipboard.SetDataObject(data);
                //begin drag and drop
                DragDrop.DoDragDrop(this.text1, data, DragDropEffects.Move);
            }
        }

        /// <summary>
        ///     Drag and drop route handler for changes in the keyboard or mouse button state during a drag-and-drop operation
        /// </summary>
        /// <param name="sender">Owener of the route event</param>
        /// <param name="e"> QueryContinueDrag data information.</param>
        private void DragSourceQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            //whe keystate is non, draop is heppen
            if (e.KeyStates == DragDropKeyStates.None)
            {
                DataObject retrievedData = (DataObject)Clipboard.GetDataObject();

                if (retrievedData.GetDataPresent("DataObjectInformation"))
                {
                    var dr = retrievedData.GetData("DataObjectInformation") as DataObjectInformation;
                    MessageBox.Show(dr.InfoValue);
                }
                    //unsubscribe event
                    this.QueryContinueDrag -= this.queryhandler;
                e.Handled = true;
                //Unhooking on Mouse Up
                InterceptMouse.UnhookWindowsHookEx(InterceptMouse.m_hookID);

                //notifiy user about drop result
                Task.Run(
                    () =>
                        {
                            //Drop hepend outside Instantly app
                            if (InterceptMouse.IsMouseOutsideApp) MessageBox.Show("Dragged outside app");
                            else MessageBox.Show("Dragged inside app");
                        });
            }
        }
    }
}