An implementation of Game of Life using an actor framework, similar to the example given in "Concurrency in .NET" by Riccardo Terrell. There, F#'s MailboxProcessor was used - this uses Microsoft Orleans.

The Game of Life implementation features 10000 nodes (100x100). By starting the silo on multiple servers (and hosting a membership table somewhere) , the computation can be distributed (ie. horizontal scaling.)


https://github.com/user-attachments/assets/16a2103c-afaf-423f-86c5-f8000ec0546e

