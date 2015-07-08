﻿using System;
using tdsm.api.Misc;
using tdsm.api.Plugin;

namespace tdsm.api.Callbacks
{
    public static class MainCallback
    {
        public static Action StatusTextChange;
        public static Action UpdateServer;

        //public static bool StartEclipse;
        //public static bool StartBloodMoon;

        public static void ProgramStart()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Tools.WriteLine("TDSM Rebind API build {0}{1}", Globals.Build, Globals.PhaseToSuffix(Globals.BuildPhase));
            Console.ForegroundColor = Command.ConsoleSender.DefaultColour;

            Globals.Touch();
            ID.Lookup.Initialise();

            PluginManager.SetHookSource(typeof(HookPoints));
            PluginManager.Initialize(Globals.PluginPath);
            PluginManager.LoadPlugins();

            if (!Permissions.PermissionsManager.IsSet)
            {
                var file = System.IO.Path.Combine(Globals.DataPath, "permissions.xml");
                //if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
                if (System.IO.File.Exists(file))
                {
                    var handler = new Permissions.XmlSupplier(file);
                    if (handler.Load())
                        Permissions.PermissionsManager.SetHandler(handler);
                }
                //else
                //{
                //    var test = new Permissions.XmlReflect()
                //    {
                //        Groups = new Permissions.XmlGroup[]
                //        { 
                //            new Permissions.XmlGroup()
                //            {
                //                Nodes = new Permissions.XmlNode[]
                //                {
                //                    new Permissions.XmlNode()
                //                    {
                //                        Deny = true,
                //                        Key = "tdsm.testa"
                //                    },
                //                    new Permissions.XmlNode()
                //                    {
                //                        Key = "tdsm.testb"
                //                    }
                //                },
                //                Name = "groupA"
                //            },
                //            new Permissions.XmlGroup()
                //            {
                //                Nodes = new Permissions.XmlNode[]
                //                {
                //                    new Permissions.XmlNode()
                //                    {
                //                        Deny = true,
                //                        Key = "tdsm.testm"
                //                    },
                //                    new Permissions.XmlNode()
                //                    {
                //                        Deny = true,
                //                        Key = "tdsm.testp"
                //                    }
                //                },
                //                Name = "groupb"
                //            }
                //        },
                //        Players = new Permissions.XmlPlayer[]
                //        { 
                //            new Permissions.XmlPlayer()
                //            {
                //                Nodes = new Permissions.XmlNode[]
                //                {
                //                    new Permissions.XmlNode()
                //                    {
                //                        Key = "tdsm.testc"
                //                    },
                //                    new Permissions.XmlNode()
                //                    {
                //                        Deny = true,
                //                        Key = "tdsm.testd"
                //                    }
                //                },
                //                Name = "playerA",
                //                Groups = new string[] {"groupa","groupb"}
                //            },
                //            new Permissions.XmlPlayer()
                //            {
                //                Nodes = new Permissions.XmlNode[]
                //                {
                //                    new Permissions.XmlNode()
                //                    {
                //                        Key = "tdsm.testcbbb"
                //                    },
                //                    new Permissions.XmlNode()
                //                    {
                //                        Deny = true,
                //                        Key = "tdsm.testd"
                //                    }
                //                },
                //                Name = "playerb"
                //            }
                //        }
                //    };
                //    var slz = new System.Xml.Serialization.XmlSerializer(typeof(Permissions.XmlReflect));
                //    using (var fs = System.IO.File.OpenWrite(file))
                //    {
                //        slz.Serialize(fs, test);
                //        fs.Flush();
                //    }
                //}
            }
        }

        public static bool OnProgramStarted(string[] cmd)
        {
            System.Threading.Thread.CurrentThread.Name = "Run";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ProgramStart();
#pragma warning disable 0162
            if (!Globals.FullAPIDefined)
            {
                Console.WriteLine("Your tdsm.api.dll is incorrect, and does not expose all methods.");
                return false;
            }
#pragma warning restore 0162

            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.ProgramStart()
            {
                Arguments = cmd
            };
            HookPoints.ProgramStart.Invoke(ref ctx, ref args);

            return ctx.Result == HookResult.DEFAULT;
        }

        public static void OnProgramFinished()
        {
            //Close the logging if set
            if (Tools.WriteClose != null) Tools.WriteClose.Invoke();
        }

        public static bool Initialise()
        {
#if Full_API
            if (Terraria.Main.dedServ)
            {
                var ctx = new HookContext()
                {
                    Sender = HookContext.ConsoleSender
                };
                var args = new HookArgs.ServerStateChange()
                {
                    ServerChangeState = ServerState.Initialising
                };
                HookPoints.ServerStateChange.Invoke(ref ctx, ref args);

                return ctx.Result != HookResult.IGNORE;
            }
#endif

            return true; //Allow continue
        }

        public static void UpdateServerEnd()
        {
            ///* Check tolled tasks */
            //Tasks.CheckTasks();

            if (UpdateServer != null)
                UpdateServer();


            for (var i = 0; i < Terraria.Netplay.Clients.Length; i++)
            {
                var client = Terraria.Netplay.Clients[i];
//                if (player.active)
                if(client != null && client.Socket != null && client.Socket is ClientConnection)
                {
                    var conn = (client.Socket as ClientConnection);
                    if (conn != null)
                        conn.Flush();
                }
            }
            //var ctx = new HookContext()
            //{
            //    Sender = HookContext.ConsoleSender
            //};
            //var args = new HookArgs.UpdateServer();
            //HookPoints.UpdateServer.Invoke(ref ctx, ref args);
        }

        public static void WorldLoadBegin()
        {
            //Since this is hook is at the end of the world loading then we can clear the new progress loading
#if Full_API
            Terraria.Main.statusText = String.Empty;
#endif

            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.ServerStateChange()
            {
                ServerChangeState = ServerState.WorldLoading
            };
            HookPoints.ServerStateChange.Invoke(ref ctx, ref args);
        }

        public static void WorldLoadEnd()
        {
            //Since this is hook is at the end of the world loading then we can clear the new progress loading
#if Full_API
            Terraria.Main.statusText = String.Empty;
#endif

            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.ServerStateChange()
            {
                ServerChangeState = ServerState.WorldLoaded
            };
            HookPoints.ServerStateChange.Invoke(ref ctx, ref args);
        }

        public static void WorldGenerateBegin()
        {
            //Since this is hook is at the end of the world loading then we can clear the new progress loading
#if Full_API
            Terraria.Main.statusText = String.Empty;
#endif

            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.ServerStateChange()
            {
                ServerChangeState = ServerState.WorldGenerating
            };
            HookPoints.ServerStateChange.Invoke(ref ctx, ref args);
        }

        public static void WorldGenerateEnd()
        {
            //Since this is hook is at the end of the world loading then we can clear the new progress loading
#if Full_API
            Terraria.Main.statusText = String.Empty;
#endif

            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.ServerStateChange()
            {
                ServerChangeState = ServerState.WorldGenerated
            };
            HookPoints.ServerStateChange.Invoke(ref ctx, ref args);
        }

        //private static int _textTimeout = 0;
        public static void OnStatusTextChange()
        {
            /* Check tolled tasks - OnStatusTextChanged is called without clients connected */
            Tasks.CheckTasks();

#if Full_API
            if (Terraria.Main.oldStatusText != Terraria.Main.statusText)
            {
                if (StatusTextChange != null)
                    StatusTextChange();
                else
                {
                    Terraria.Main.oldStatusText = Terraria.Main.statusText;
                    Tools.WriteLine(Terraria.Main.statusText);
                }
                /*var ctx = new HookContext()
                {
                    Sender = HookContext.ConsoleSender
                };
                var args = new HookArgs.StatusTextChanged() { };
                HookPoints.StatusTextChanged.Invoke(ref ctx, ref args);

                if (ctx.Result == HookResult.DEFAULT)
                {
                    Terraria.Main.oldStatusText = Terraria.Main.statusText;
                    Tools.WriteLine(Terraria.Main.statusText);
                }*/
                //_textTimeout = 0;
            }
            //else if (Terraria.Main.oldStatusText == String.Empty && Terraria.Main.statusText == String.Empty)
            //{
            //    if (_textTimeout++ > 1000)
            //    {
            //        _textTimeout = 0;
            //        Terraria.Main.statusText = String.Empty;
            //    }
            //}
#endif
        }
    }
}

