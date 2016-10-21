/*

The MIT License (MIT)

Copyright (c) 2016 Allen Vanderlinde

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BbWsClient;

/**
 * @brief   .NET web services client that pulls various data from
 *          a Blackboard Learn environment.
 */
namespace wsclient
{
    /**
     * @brief   Application execution starts here.
     */
    public class Program
    {
        static int Main(string[] args)
        {
            /* For the purpose of this Getting Started Guide,
             * we'll be using command-line arguments to determine
             * what data we're looking to pull. */
            string host, sharedSecret, userId;
            try
            {
                host = args[0];
                sharedSecret = args[1];
                userId = args[2];
            }
            catch(Exception)
            {
                Console.WriteLine("Error: Necessary arguments not given.");

                return -1;
            }

            Console.WriteLine("\nConnecting to host {0}...", host);

            /* Establish a connection to the environment
                before any registration or calls. */
            WebServices.connectAndInitialize(host);

            /* Register our tool for
                the first time if
                specified. */
            if(userId.ToLower().Equals("register"))
            {
                try
                {
                    if(!WebServices.register("This is my web services tool.", args[3], sharedSecret))
                    {
                        Console.WriteLine("\nUnable to register tool in host {0}.\n\tMore info: {1}\n\t*Global password may be invalid.", host, WebServices.getWS().getLastError());

                        return -1;
                    }
                    else
                    {
                        Console.WriteLine("\nRegistered tool successfully!");
                        WebServices.logout();

                        return 0;
                    }
                }
                catch(IndexOutOfRangeException)
                {
                    Console.WriteLine("\nError: Not all arguments given for registration.");
                    WebServices.logout();

                    return -1;
                }
            }

            /* Log into the environment
                as a proxy tool. */
            if (!WebServices.login(sharedSecret))
            {
                Console.WriteLine("\nError: Unable to log into host.\n\tMore Info: {0}", WebServices.getWS().getLastError());

                return -1;
            }
            Console.WriteLine("Logged into host as proxy tool successfully.");

            /* Initialize the web
                services wrappers
                we need. */
            Console.WriteLine("\nInitializing web services...");
            if(!WebServices.initializeWrappers())
            {
                Console.WriteLine("Error: Unable to initialize one or more web services: {0}", WebServices.getWS().getLastError());

                return -1;
            }
            else
                Console.WriteLine("Initialized web services successfully.");

            /* Send the request and
                store the data we're
                looking for. */
            bool success = false;

            Console.WriteLine("\nLooking up user \"{0}\"...", userId);
            DataRequest request = new DataRequest(userId, out success);
            
            if(!success)
            {
                Console.WriteLine("\nError: Data for user ID \"{0}\" was unable to be pulled.\n\tMore info: {1}", userId, WebServices.getWS().getLastError());
                WebServices.logout();

                return -1;
            }

            /* Display some basic user data
                in Shapshot FlatFile format. */
            UserVO user = request.getUserData(0);
            Console.WriteLine("\nResults:");
            Console.WriteLine("DATA_SOURCE_KEY|EXTERNAL_PERSON_KEY|USER_ID|FIRSTNAME|LASTNAME|STUDENT_ID|BIRTHDATE");
            Console.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                WebServices.getDataSource(user.dataSourceId),
                user.userBatchUid,
                user.name,
                user.extendedInfo.givenName,
                user.extendedInfo.familyName,
                user.studentId,
                DataRequest.toStandardDate(user.birthDate));

            /* Log out. */
            Console.WriteLine();
            WebServices.logout();

            return 0;
        }
    }
}
