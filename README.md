CMPP30
======

China mobile SMS gateway protocol C# implementation (CMPP 3.0) for short messaging. This is a non-standard CMPP30 short message client, which is already heavily used in our cloud messaging platform. This open-source version is free to use. I will include fixes in the future and you're welcome to contribute to it.

This client is currently built with Visual Studio 2012. And is also compatible with mono.

## Features

+ Sending short messages
+ Sending long messages
+ Receiving message report
+ Receiving short messages
+ Remove gateway signature
+ Auto-reconnect

## To-do list

+ Standardize client interface for general usage (e.g. dynamic configuration, removing hard-code limitations)
+ Multiple connection
+ Receiving long message reports (currently using the first report)
+ Batch send (and receive message reports)
+ Flow control
+ Other improvements, e.g. avoid using threads and locks, optimize windows size control, etc.

## Contributing

You're welcome to integrate this client into your systems as well as contributing to this client.

1. Fork it.
2. Create a branch (`git checkout -b improvement`)
3. Commit your changes (`git commit -am "Add sending interface dynamic configuration."`)
4. Push to the branch (`git push origin improvement`)
5. Open a Pull Request
6. Enjoy a refreshing Diet Coke and wait
