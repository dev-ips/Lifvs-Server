$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#gridStoreInventory").jqGrid({
        url: '../StoreInventory/GetStoreInventory?storeId=' + $("#StoreId").val(),
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Store', 'Inventory', 'StoreUnit', ''],
        colModel: [
            { key: true, hidden: true, name: 'Id', index: 'Id', editable: false, sortable: false },
            { key: false, hidden:true, name: 'Store', index: 'Store', editable: false, sortable: false, width: 60, search: false },
            { key: false, name: 'Inventory', index: 'Inventory', editable: false, sortable: false, width: 60, search: false },
            { key: false, name: 'NumberOfItems', index: 'NumberOfItems', editable: false, sortable: false, width: 20, search: false },

            { name: 'Id', search: false, index: 'Id', sortable: false, width: 20, formatter: deleteStoreInventoryLink }
        ],
        //pager: jQuery('#pager'),
        rowNum: -1,
        //rowList: [10, 20, 30, 40],
        viewrecords: true,
        height: '100%',
        width: '100%',
        sortname: 'Store',
        sortorder: "Asc",
        scrollOffset: 0,
        loadonce: false,
        shrinkToFit: true,
        forceFit: true,
        emptyrecords: 'No records to display',
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            Id: "0"
        },
        loadComplete: function () {

            //just make all the row to editmode
            ids = $("#gridStoreInventory").jqGrid('getDataIDs');
            var l = ids.length;
            for (var i = 0; i < l; i++) {
                $("#gridStoreInventory").jqGrid('editRow', ids[i], true);
            }

        },
        autowidth: true
    }).navGrid('#pager', { edit: false, add: false, del: false, search: false, refresh: false, searchtext: "Search From here", autowidth: true },
        {
        },
        {}, // add options
        {}, // delete options
        {
            multipleSearch: true
        });

    function deleteStoreInventoryLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDeleteStoreInventory' class='btn btn-danger btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DeleteStoreInventory.call(this)'>Delete</a>";
    }

});