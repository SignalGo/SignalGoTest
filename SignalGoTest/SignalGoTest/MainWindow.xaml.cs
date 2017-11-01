﻿using Newtonsoft.Json;
using SignalGo.Client;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SignalGoTest
{
    /// <summary>
    /// my class comment
    /// </summary>
    public class MyClass
    {
        /// <summary>
        /// یک متد مثال است
        /// </summary>
        /// <param name="name">نام شخص مورد نظر</param>
        /// <param name="age">سن شخص مورد نظر</param>
        /// <returns>متن پیام می باشد</returns>
        /// <exception cref="MainWindow.btnconnect">وقتی کلاس پروفایل خالی باشد یا نام یا نام خانوادگی وارد نشده باشد</exception>
        /// <exception cref="MainWindow.txtAddress">خطای الکی</exception>
        public string ExampleProperty(string name, int age)
        {
            return "ascass";
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //try
            //{
            //    using (XamlCommentLoader loader = new XamlCommentLoader())
            //    {
            //        var method = typeof(MyClass).GetMethod("ExampleProperty");
            //        var documentation = loader.GetCommment(method);

            //    }


            //    //var doc = XElement.Load(@"D:\Github\Atitec.OffseeAPI\Atitec.OffseeAPI\Atitec.OffseeAPI.ConsoleServer\bin\Debug\Atitec.Models.xml");
            //    //var comments = doc.DescendantNodes().ToList();

            //    //foreach (var comment in comments)
            //    //{
            //    //    //comment.na
            //    //    var type = comment.GetType().ToString();
            //    //}
            //}
            //catch (Exception ex)
            //{


            //}
            Closing += MainWindow_Closing;
            InitializeComponent();
            LoadData();

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        List<object> fullItems { get; set; }

        ClientProvider provider = null;
        private void btnconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                provider = new ClientProvider();
                provider.OnDisconnected = () =>
                {
                    btndisconnect_Click(null, null);
                };
                provider.Connect(txtAddress.Text);
                var result = provider.GetListOfServicesWithDetials(txtAddress.Text);
                result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.Name).ToList();
                if (result.Services.Count > 0)
                    provider.RegisterClientServiceInterface(result.Services.FirstOrDefault().ServiceName);

                btnconnect.IsEnabled = false;
                btndisconnect.IsEnabled = true;
                //foreach (var serviceDetails in result)
                //{
                //    foreach (var serviceInterface in serviceDetails.Services)
                //    {
                //        foreach (var method in serviceInterface.Methods)
                //        {
                //            foreach (var parameter in method.Parameters)
                //            {
                //                //parameter.Name
                //            }
                //        }
                //    }
                //}
                //List<MethodItemInfo> items = new List<SignalGoTest.MethodItemInfo>();
                //foreach (var server in result)
                //{
                //    foreach (var service in server.Services)
                //    {
                //        foreach (var method in service.Methods)
                //        {
                //            var m = new SignalGoTest.MethodItemInfo() { Name = method.MethodName };
                //            m.Items = new List<ParameterItemInfo>();
                //            if (method.Parameters != null)
                //                foreach (var p in method.Parameters)
                //                {
                //                    m.Items.Add(new ParameterItemInfo() { Name = p.Name, Type = p.Type });
                //                }
                //            items.Add(m);
                //        }
                //    }
                //}
                UpdateData((ProviderDetailsInfo)TreeViewServices.DataContext, result);
                TreeViewServices.DataContext = result;
                List<object> items = new List<object>();
                items.AddRange(result.Services);
                items.Add(result.WebApiDetailsInfo);
                items.Add(result.ProjectDomainDetailsInfo);
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
                fullItems = items;
                //SelectTreeViewOldItem();
                //SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, ServiceName = txtServiceName.Text, Items = items });
            }
            catch (Exception ex)
            {
                provider.Dispose();
                provider = null;
                btnconnect.IsEnabled = true;
                btndisconnect.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }

        public void UpdateData(ProviderDetailsInfo oldData, ProviderDetailsInfo newData)
        {
            if (oldData == null || newData == null)
                return;
            foreach (var server in oldData.Services)
            {
                var findServer = newData.Services.FirstOrDefault(x => x.FullNameSpace == server.FullNameSpace);
                if (findServer == null)
                    continue;
                findServer.IsExpanded = server.IsExpanded;
                findServer.IsSelected = server.IsSelected;
                foreach (var service in server.Services)
                {
                    var findService = findServer.Services.FirstOrDefault(x => x.FullNameSpace == service.FullNameSpace);
                    if (findService != null)
                    {
                        findService.IsExpanded = service.IsExpanded;
                        findService.IsSelected = service.IsSelected;
                    }
                    foreach (var method in service.Methods)
                    {
                        if (findService == null)
                            continue;
                        var find = (from x in findService.Methods where x.MethodName == method.MethodName && x.Parameters.Count == method.Parameters.Count select x).FirstOrDefault();
                        if (find != null)
                        {
                            find.IsExpanded = method.IsExpanded;
                            find.IsSelected = method.IsSelected;

                            foreach (var parameter in method.Parameters)
                            {
                                var p = (from x in find.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                if (p != null)
                                {
                                    parameter.IsExpanded = p.IsExpanded;
                                    parameter.IsSelected = p.IsSelected;
                                    p.IsJson = parameter.IsJson;
                                    p.Value = parameter.Value?.ToString();
                                    p.TemplateValue = parameter.TemplateValue?.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SaveData(AppDataInfo data)
        {
            try
            {
                var serial = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                File.WriteAllText(path, serial, Encoding.UTF8);
            }
            catch (Exception ex)
            {

            }
        }

        public void LoadData()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                var appdata = Newtonsoft.Json.JsonConvert.DeserializeObject<AppDataInfo>(File.ReadAllText(path, Encoding.UTF8));
                txtAddress.Text = appdata.ServerAddress;
                //txtServiceName.Text = appdata.ServiceName;
                List<object> items = new List<object>();
                items.AddRange(appdata.Items.Services);
                items.Add(appdata.Items.WebApiDetailsInfo);
                items.Add(appdata.Items.ProjectDomainDetailsInfo);
                TreeViewServices.DataContext = appdata.Items;
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
                fullItems = items;
                lstHistoryCalls.ItemsSource = appdata.Histories;
            }
            catch (Exception ex)
            {

            }
        }

        private void btndisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (provider != null)
            {
                provider.OnDisconnected = null;
                provider.Dispose();
            }
            provider = null;
            Dispatcher.Invoke(new Action(() =>
            {
                btndisconnect.IsEnabled = false;
                btnconnect.IsEnabled = true;
            }));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, Items = (ProviderDetailsInfo)TreeViewServices.DataContext, Histories = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TreeViewServices.SelectedItem == null || !(TreeViewServices.SelectedItem is ServiceDetailsMethod))
                    return;
                btnSend.IsEnabled = false;
                var selectedMethod = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                var serviceName = ((ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).FirstOrDefault()).ServiceName;
                ServiceDetailsMethod sendReq = new ServiceDetailsMethod();
                sendReq.MethodName = selectedMethod.MethodName;
                sendReq.Parameters = new List<ServiceDetailsParameterInfo>();
                if (selectedMethod.Parameters != null)
                {
                    foreach (var item in selectedMethod.Parameters)
                    {
                        sendReq.Parameters.Add(new ServiceDetailsParameterInfo() { Name = item.Name, Type = item.Type, Value = item.IsJson ? item.Value : JsonConvert.SerializeObject(item.Value), IsJson = item.IsJson });
                    }
                }
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string request = "";
                        var response = provider.SendRequest(serviceName, sendReq, out request);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            var history = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource;
                            if (history == null)
                                history = new List<SignalGoTest.HistoryCallInfo>();
                            response = response == null ? "void ok" : FormatJson(response);
                            history.Insert(0, new HistoryCallInfo() { CallDateTime = DateTime.Now, MethodName = sendReq.MethodName, Request = FormatJson(request), Response = response });
                            lstHistoryCalls.ItemsSource = null;
                            lstHistoryCalls.ItemsSource = history;
                            txtReponse.Text = response;
                            btnSave_Click(null, null);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            txtReponse.Text = ex.Message + Environment.NewLine + ex.StackTrace;
                        }));
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        btnSend.IsEnabled = true;
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("error", ex.Message);
                btnSend.IsEnabled = true;
            }
        }

        private void loadTemplate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            try
            {
                if (TreeViewServices.SelectedItem == null || !(TreeViewServices.SelectedItem is ServiceDetailsMethod))
                    return;
                var column = DGRequestValues.Columns[2] as DataGridTextColumn;
                DataGridRow row = DGRequestValues.ItemContainerGenerator.ContainerFromIndex((DGRequestValues.SelectedIndex)) as DataGridRow;
                var cellTextBox = ((TextBlock)(column.GetCellContent(row)));

                btn.IsEnabled = false;
                var serviceName = ((ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).FirstOrDefault()).ServiceName;
                var method = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                var parameter = (ServiceDetailsParameterInfo)btn.DataContext;
                var paramIndex = method.Parameters.IndexOf(parameter);
                MethodParameterDetails sendReq = new MethodParameterDetails();
                sendReq.MethodName = method.MethodName;
                sendReq.ParametersCount = method.Parameters.Count;
                sendReq.ServiceName = serviceName;
                sendReq.ParameterIndex = paramIndex;

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var response = provider.GetMethodParameterDetial(sendReq);
                        Dispatcher.Invoke(new Action(() =>
                        {
                            parameter.TemplateValue = FormatJson(response);
                            BindingExpression bindingExpression = DGRequestValues.GetBindingExpression(DataGrid.ItemsSourceProperty);
                            Binding parentBinding = bindingExpression.ParentBinding;

                            var source = DGRequestValues.ItemsSource;
                            DGRequestValues.ItemsSource = null;
                            DGRequestValues.ItemsSource = source;
                            DGRequestValues.SetBinding(DataGrid.ItemsSourceProperty, parentBinding);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            parameter.TemplateValue = ex.Message + Environment.NewLine + ex.StackTrace;
                            BindingExpression bindingExpression = DGRequestValues.GetBindingExpression(DataGrid.ItemsSourceProperty);
                            Binding parentBinding = bindingExpression.ParentBinding;

                            var source = DGRequestValues.ItemsSource;
                            DGRequestValues.ItemsSource = null;
                            DGRequestValues.ItemsSource = source;
                            DGRequestValues.SetBinding(DataGrid.ItemsSourceProperty, parentBinding);
                        }));
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        btn.IsEnabled = true;
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("error", ex.Message);
                btn.IsEnabled = true;
            }
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private void btnToString_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtReponse.Text = JsonConvert.DeserializeObject<string>(txtReponse.Text);
            }
            catch (Exception ex)
            {
                txtReponse.Text = ex.ToString();
            }
        }

        Type SelectedItemType { get; set; }
        object SelectedItem { get; set; }
        string SelectedUniqName { get; set; }
        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                VirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as VirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(new Rect(i, i, i, i));

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }
        private TreeViewItem GetTreeViewItemByName(ItemsControl container, string uniqName)
        {
            if (container != null)
            {
                if (GetObjectUniqName(container.DataContext) == uniqName)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                VirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as VirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(new Rect(i, i, i, i));

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItemByName(subContainer, uniqName);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }

        public void SaveTreeViewSelectedItem()
        {
            if (TreeViewServices.SelectedItem == null)
                return;
            SelectedItem = TreeViewServices.SelectedItem;
            if (SelectedItem == null)
                SelectedItemType = null;
            else
            {
                SelectedItemType = SelectedItem.GetType();
                SelectedUniqName = GetObjectUniqName(SelectedItem);

            }

        }

        public void SelectTreeViewOldItem()
        {
            if (SelectedItem == null)
                return;
            var find = GetTreeViewItem(TreeViewServices, SelectedItem);
            if (find != null)
                find.IsSelected = true;
            else
            {
                find = GetTreeViewItemByName(TreeViewServices, SelectedUniqName);
                if (find != null)
                    find.IsSelected = true;
            }
        }

        public string GetObjectUniqName(object obj)
        {
            if (obj.GetType() == typeof(HttpControllerDetailsInfo))
            {
                var result = (HttpControllerDetailsInfo)obj;
                return result.GetType().FullName + result.Url;
            }
            else if (obj.GetType() == typeof(ModelDetailsInfo))
            {
                var result = (ModelDetailsInfo)obj;
                return result.GetType().FullName + result.FullNameSpace + result.Name;
            }
            else if (obj.GetType() == typeof(ProjectDomainDetailsInfo))
            {
                var result = (ProjectDomainDetailsInfo)obj;
                return result.GetType().FullName;
            }
            else if (obj.GetType() == typeof(ProviderDetailsInfo))
            {
                var result = (ProviderDetailsInfo)obj;
                return result.GetType().FullName;
            }
            else if (obj.GetType() == typeof(ServiceDetailsInfo))
            {
                var result = (ServiceDetailsInfo)obj;
                return result.GetType().FullName + result.FullNameSpace + result.ServiceName + result.NameSpace;
            }
            else if (obj.GetType() == typeof(ServiceDetailsInterface))
            {
                var result = (ServiceDetailsInterface)obj;
                return result.GetType().FullName + result.FullNameSpace + result.NameSpace;
            }
            else if (obj.GetType() == typeof(ServiceDetailsMethod))
            {
                var result = (ServiceDetailsMethod)obj;
                return result.GetType().FullName + result.MethodName + result.Parameters.Count + result.ReturnType;
            }
            else if (obj.GetType() == typeof(ServiceDetailsParameterInfo))
            {
                var result = (ServiceDetailsParameterInfo)obj;
                return result.GetType().FullName + result.Name + result.FullTypeName + result.Type;
            }
            else if (obj.GetType() == typeof(WebApiDetailsInfo))
            {
                var result = (WebApiDetailsInfo)obj;
                return result.GetType().FullName;
            }
            return Guid.NewGuid().ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = ((TextBox)sender).Text;
            if (string.IsNullOrEmpty(value))
            {
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = fullItems;
            }
            else
            {
                TreeViewServices.ItemsSource = null;
                List<object> newItems = new List<object>();
                foreach (var item in fullItems)
                {
                    ServiceDetailsInfo clone = null;
                    bool canAdd = false;
                    if (item.GetType() == typeof(ServiceDetailsInfo))
                    {
                        var result = (ServiceDetailsInfo)item;
                        clone = result.Clone();

                        foreach (var service in result.Services)
                        {
                            var cloneService = service.Clone();
                            foreach (var method in service.Methods)
                            {
                                if (method.MethodName.ToLower().Contains(value))
                                {
                                    if (!clone.Services.Contains(cloneService))
                                        clone.Services.Add(cloneService);
                                    cloneService.Methods.Add(method);
                                    canAdd = true;
                                    continue;
                                }

                                foreach (var p in method.Parameters)
                                {
                                    if (p.Name.ToLower().Contains(value))
                                    {
                                        if (!clone.Services.Contains(cloneService))
                                            clone.Services.Add(cloneService);
                                        cloneService.Methods.Add(method);
                                        canAdd = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (canAdd)
                        newItems.Add(clone);
                }
                TreeViewServices.ItemsSource = newItems;
            }
        }

        private void TreeViewServices_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //SaveTreeViewSelectedItem();
        }
    }
}
