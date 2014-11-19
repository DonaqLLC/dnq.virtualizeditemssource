dnq.virtualizeditemssource
==========================

A virtualized items source implementation that provides necessary logic for fetching and caching a linear/sequential view into large data set (item-source)

How To Use
--------------------------

The VirtualizedItemSource<T> class is the primary component in this project. It serves as an adapter to a collection of items and it abstracts the necessary operations for lazy fetching of data (out-of-band) and pagination as well as pre-loading and caching. 

The user can configure the number of data items to pre load, via a parameter in the constructor.

To use the class the user only needs to provide two delegates:

 * an action that returns a set of data from the underlying data source, given the paging information
 * an action that returns the total count of available data from the undersying data source

The declaration looks something like this:

    private DNQ.VirtualizedItemsSource.VirtualizedItemSource<DataItem> virtualItemsSource;
    
And the constructor should look something like this:

    virtualItemsSource = new DNQ.VirtualizedItemsSource.VirtualizedItemSource<DataItem>(500, 
                                                                     CountItems, 
                                                                     RetrieveItems, 
                                                                     LogMessages);
                                                                     
* The first parameter specifies how many items to load at a time. This should be slightly larger than what is visible on the UI (so if the UI can display between 100 and 200 items, a good value might be 500).

* The second and third parameters are delegates that are responsible with counting the number of available data items, and actually retrieving the data items from the underlying data source.

* The last paramter is a delegate that may be used to trace debug information. It's a good idea to provide a simple implementation for debugging purposes that is only enabled in Debug mode.

The **count** and **retrieve** delegates might look something like this:

    int CountItems()
    {
        /* Query Database To Determine Total Number Of Items */
        return __sourceDataSetCount__;
    }


    private IEnumerable<DataItem> RetrieveItems(int offset, 
                                                int count, 
                                                System.Threading.CancellationToken cancellationToken)
    {            
        var lst = new List<DataItem>();

        /* query database to retrieve [count] number of items, starting at offset [offset] */
        /*   - add the results to the [lst] list */
        /*   - the [cancellationToken] may be used to check whether cancellation was requested */

        return lst;
    }

    void LogMessages(int level, string message)
    {
        if (level > 2)
            Console.WriteLine("[INFO]: {0}", message);
        else 
            Console.WriteLine("{1}: {0}", message, (level == 2 ? "[WARN]" : "[ERROR]"));
    }

Finally, the output surface of the component is pretty simple. It provides a `Get(int index)` method for retriving an item, given its index, and a `TotalCount` property which gives the number of items. This should be sufficient for plugging it into a WinForms and WPF application as an items source.

**Note** The virtualized items source will handle all necessary logic for ensuring that items are preloaded and avaialbe when the `Get()` method is called, behind the scenes. It also supports underlying collections that change dynamically while being displayed (ie. where items are added or removed).

The logic to retrieve an item might look something like this:

    DataItem dataItem = null;
    if (virtuaItemsSource.GetItem(itemIndex, out dataItem))     // tries to retrieve the item
    {
        item = MakeListItem(demoItem);
    }
    else                                                        // this method fails if the underlying 
    {                                                           // collection has changed since last query
        // it probably means that the underlying collection has changed..
        //     so force a requery, and notify the UI component
        lstItems.VirtualListSize = virtuaItemsSource.TotalCount;
        item = MakeListItem(null);
    }
    return item;   // return the UI element to the UI component
