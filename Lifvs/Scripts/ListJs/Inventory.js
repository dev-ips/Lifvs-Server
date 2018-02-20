$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#gridInventory").jqGrid({
        url: 'InventoryWeb/GetAllInventory',
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Brand', 'Name', 'Inventory Code', 'Supplier', 'Volume','VolumeType', '', ''],
        colModel: [
            { key: true, hidden: true, name: 'Id', index: 'Id', editable: false, sortable: false },
            { key: false, name: 'BrandName', index: 'BrandName', editable: false, sortable: false, width: 60, search: false },
            { key: false, name: 'Name', index: 'Name', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'InventoryCode', index: 'InventoryCode', editable: false, sortable: false, width: 40, search: false },
            { key: false, name: 'Supplier', index: 'Supplier', editable: false, sortable: false, width: 40, search: false },
            { key: false, name: 'Volume', index: 'Volume', editable: false, sortable: false, width: 40, search: false },
            { key: false, name: 'VolumeType', index: 'VolumeType', editable: false, sortable: false, width: 40, search: false },
            { name: 'Id', index: 'Id', editable: false, sortable: false, width: 30, formatter: editInventoryLink },
            { name: 'Id', search: false, index: 'Id', sortable: false, width: 30, formatter: deleteInventoryLink }
        ],
        //pager: jQuery('#pager'),
        rowNum: -1,
        //rowList: [10, 20, 30, 40],
        viewrecords: true,
        height: '100%',
        width: '100%',
        sortname: 'Brand',
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
            ids = $("#gridInventory").jqGrid('getDataIDs');
            var l = ids.length;
            for (var i = 0; i < l; i++) {
                $("#gridInventory").jqGrid('editRow', ids[i], true);
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

    function editInventoryLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkEditInventory' class='btn btn-success btn-block btn-sm' style='cursor:pointer;color:white;' onClick='EditInventory.call(this)'>Edit</a>";
    }
    function deleteInventoryLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDeleteInventory' class='btn btn-danger btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DeleteInventory.call(this)'>Delete</a>";
    }

});