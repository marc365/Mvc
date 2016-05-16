        public ActionResult ExportRoutes()
        {
            if (!_permissionService.Authorize(PageRedirectPermissionProvider.AccessPageRedirect))
                return AccessDeniedView();

            var query = _pageredirectRepository.Table.Select(x => x).ToList();

            string fileName = String.Format("routes_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            MemoryStream memoryStream = new MemoryStream();
            
            using (CsvFileWriter writer = new CsvFileWriter(memoryStream))
            {
                foreach (RouteBase Item in RouteTable.Routes)
                {
                    CsvRow row = new CsvRow();


                    RouteMap route = new RouteMap();

                    route.Name = RouteName(Item);

                    if ((Item as Route).Defaults != null)
                    {
                        foreach (var token in (Item as Route).Defaults)
                        {
                            if (token.Key == "controller")
                            {
                                route.Controller = (token.Value as string);
                            }
                            else if (token.Key == "action")
                            {
                                route.Action = (token.Value as string);
                            }
                            else
                            {
                                route.Parameter += (token.Value as string);
                            }
                        }
                    }

                    route.Url = (Item as Route).Url;

                    row.Add(route.Name);
                    row.Add(route.Url);
                    row.Add(route.Controller);
                    row.Add(route.Action);
                    row.Add(route.Parameters);

                    writer.WriteRow(row);
                }
            }

            return File(memoryStream.ToArray(), "text/csv", fileName);
        }

        [NonAction]
        public string RouteName(RouteBase original)
        {
            var routes = System.Web.Routing.RouteTable.Routes;

            if (routes.Contains(original))
            {
                var namedMapField = routes.GetType().GetField("_namedMap", BindingFlags.NonPublic | BindingFlags.Instance);
                var namedMap = namedMapField.GetValue(routes) as Dictionary<string, RouteBase>;

                var query =
                    from pair in namedMap
                    where pair.Value == original
                    select pair.Key;

                if (query.Count() > 0)
                {
                    return query.Single();
                }
                else
                {
                    return "Query was null";
                }
            }

            return string.Empty;
        }
