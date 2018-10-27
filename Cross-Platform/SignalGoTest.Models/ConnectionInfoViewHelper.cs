using SignalGo.Client;
using SignalGo.Shared;
using SignalGo.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class ConnectionInfoViewHelper
    {
        public ClientProvider Provider { get; private set; } = new ClientProvider();

        public ConnectionInfoViewHelper()
        {

        }

        public async Task CalculateDetails(ProviderDetailsInfo result, ProviderDetailsInfo oldData , string search)
        {
            DoOrder(result);
            result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.Name).ToList();
            UpdateData(oldData, result);

            //AsyncActions.RunOnUI(() =>
            //{
            //    ConnectionData connectionData = (ConnectionData)DataContext;
            //    connectionData.Items = result;
            //    UpdateData(oldData, result);
            //    result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.ObjectType).ToList();
            //    TreeViewServices.DataContext = result;
            //    List<object> items = new List<object>();
            //    items.AddRange(result.Services);
            //    items.AddRange(result.Callbacks);
            //    items.Add(result.WebApiDetailsInfo);
            //    items.Add(result.ProjectDomainDetailsInfo);
            //    TreeViewServices.ItemsSource = null;
            //    TreeViewServices.ItemsSource = items;
            //    fullItems = items;
            //    MainWindow.SaveData();
            //    busyIndicator.IsBusy = false;
            //});
            //if (!string.IsNullOrEmpty(search))
            //    await Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        TextBox_TextChanged(txtSearch, null);
            //        SelectTreeViewOldItem();
            //    }));
        }


        public static void DoOrder(ProviderDetailsInfo result)
        {
            if (result == null || result.Services == null)
                return;
            foreach (ServiceDetailsInfo serviceClass in result.Services)
            {
                foreach (ServiceDetailsInterface interfaceInfo in serviceClass.Services)
                {
                    interfaceInfo.Methods = interfaceInfo.Methods?.OrderBy(x => x.MethodName).ToList();
                }
            }
        }

        public void UpdateData(ProviderDetailsInfo oldData, ProviderDetailsInfo newData)
        {
            if (oldData == null || newData == null)
                return;
            newData.IsExpanded = oldData.IsExpanded;
            newData.IsSelected = oldData.IsSelected;

            foreach (ServiceDetailsInfo server in oldData.Services)
            {
                ServiceDetailsInfo findServer = newData.Services.FirstOrDefault(x => x.FullNameSpace == server.FullNameSpace);
                if (findServer == null)
                    continue;
                //if (_oldSelectedItem == server)
                //    _newSelectedItem = findServer;
                findServer.IsExpanded = server.IsExpanded;
                findServer.IsSelected = server.IsSelected;
                foreach (ServiceDetailsInterface service in server.Services)
                {
                    ServiceDetailsInterface findService = findServer.Services.FirstOrDefault(x => x.FullNameSpace == service.FullNameSpace);
                    if (findService != null)
                    {
                        findService.IsExpanded = service.IsExpanded;
                        findService.IsSelected = service.IsSelected;
                        //if (_oldSelectedItem == service)
                        //    _newSelectedItem = findService;
                    }

                    foreach (ServiceDetailsMethod method in service.Methods)
                    {
                        if (findService == null)
                            continue;
                        ServiceDetailsMethod find = (from x in findService.Methods where x.MethodName == method.MethodName && x.Requests.First().Parameters.Count == method.Requests.First().Parameters.Count select x).FirstOrDefault();
                        if (find != null)
                        {
                            //if (_oldSelectedItem == method)
                            //    _newSelectedItem = find;
                            find.IsExpanded = method.IsExpanded;
                            find.IsSelected = method.IsSelected;
                            foreach (ServiceDetailsRequestInfo request in method.Requests)
                            {
                                ServiceDetailsRequestInfo findRequest = find.Requests.FirstOrDefault(x => x.Name == request.Name);
                                if (findRequest == null)
                                {
                                    ServiceDetailsRequestInfo clonedReq = request.Clone();
                                    ServiceDetailsRequestInfo defRequest = find.Requests.FirstOrDefault();
                                    foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                                    {
                                        ServiceDetailsParameterInfo p = (from x in defRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                        if (p != null)
                                        {
                                            //if (_oldSelectedItem == parameter)
                                            //    _newSelectedItem = p;
                                            //p.IsExpanded = parameter.IsExpanded;
                                            //p.IsJson = parameter.IsJson;
                                            //p.IsSelected = parameter.IsSelected;
                                            //p.TemplateValue = parameter.TemplateValue;
                                            //p.Value = parameter.Value;
                                            parameter.Type = p.Type;
                                            parameter.FullTypeName = p.FullTypeName;
                                            parameter.Comment = p.Comment;
                                            clonedReq.Parameters.Add(parameter.Clone());
                                        }
                                    }
                                    find.Requests.Add(clonedReq);
                                }
                                else
                                {
                                    foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                                    {
                                        ServiceDetailsParameterInfo p = (from x in findRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                        if (p != null)
                                        {
                                            //if (_oldSelectedItem == parameter)
                                            //    _newSelectedItem = p;
                                            parameter.IsExpanded = p.IsExpanded;
                                            parameter.IsSelected = p.IsSelected;
                                            p.IsJson = parameter.IsJson;
                                            p.Value = parameter.Value?.ToString();
                                            p.TemplateValue = parameter.TemplateValue?.ToString();
                                            p.Type = parameter.Type;
                                            p.FullTypeName = parameter.FullTypeName;
                                            p.Comment = parameter.Comment;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            newData.WebApiDetailsInfo.IsExpanded = oldData.WebApiDetailsInfo.IsExpanded;
            newData.WebApiDetailsInfo.IsSelected = oldData.WebApiDetailsInfo.IsSelected;

            foreach (HttpControllerDetailsInfo server in oldData.WebApiDetailsInfo.HttpControllers)
            {
                HttpControllerDetailsInfo findServer = newData.WebApiDetailsInfo.HttpControllers.FirstOrDefault(x => x.Url == server.Url);
                if (findServer == null)
                    continue;
                findServer.IsExpanded = server.IsExpanded;
                findServer.IsSelected = server.IsSelected;
                foreach (ServiceDetailsMethod method in server.Methods)
                {
                    ServiceDetailsMethod find = (from x in findServer.Methods where x.MethodName == method.MethodName && x.Requests.First().Parameters.Count == method.Requests.First().Parameters.Count select x).FirstOrDefault();
                    if (find != null)
                    {
                        find.IsExpanded = method.IsExpanded;
                        find.IsSelected = method.IsSelected;
                        foreach (ServiceDetailsRequestInfo request in method.Requests)
                        {
                            ServiceDetailsRequestInfo findRequest = find.Requests.FirstOrDefault(x => x.Name == request.Name);
                            if (findRequest == null)
                            {
                                findRequest = request.Clone();
                                foreach (ServiceDetailsParameterInfo item in request.Parameters)
                                {
                                    findRequest.Parameters.Add(item.Clone());
                                }
                                find.Requests.Add(findRequest);
                            }
                            foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                            {
                                ServiceDetailsParameterInfo p = (from x in findRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
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
    }
}
