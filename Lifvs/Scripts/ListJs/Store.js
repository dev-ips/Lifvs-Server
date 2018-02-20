$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#gridStore").jqGrid({
        url: 'Stores/GetStores',
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Name', 'Email', 'SupervisorName', 'Phone', 'Address', 'City', 'PostalCode', '', '', ''],
        colModel: [
            { key: true, hidden: true, name: 'Id', index: 'Id', editable: false, sortable: false },
            { key: false, name: 'Name', index: 'Name', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'Email', index: 'Email', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'SupervisorName', index: 'SupervisorName', editable: false, sortable: false, width: 80, search: false },
            { key: false, name: 'Phone', index: 'Phone', editable: false, sortable: false, width: 50, search: false },
            { key: false, name: 'Address', index: 'Address', editable: false, sortable: false, width: 180, search: false },
            { key: false, name: 'City', index: 'City', editable: false, sortable: false, width: 80, search: false },
            { key: false, name: 'PostalCode', index: 'PostalCode', editable: false, sortable: false, width: 40, search: false },

            { name: 'Id', index: 'Id', editable: false, sortable: false, width: 30, formatter: editStoreLink },
            { name: 'Id', search: false, index: 'Id', sortable: false, width: 30, formatter: deleteStoreLink },
            { name: 'Id', search: false, index: 'Id', sortable: false, width: 60, formatter: downloadStoreCodeLink }
        ],
        //pager: jQuery('#pager'),
        rowNum: -1,
        //rowList: [10, 20, 30, 40],
        viewrecords: true,
        height: '100%',
        width: '100%',
        sortname: 'Name',
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
            ids = $("#gridStore").jqGrid('getDataIDs');
            var l = ids.length;
            for (var i = 0; i < l; i++) {
                $("#gridStore").jqGrid('editRow', ids[i], true);
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

    function editStoreLink(cellValue, options, rowdata, action) {
        return "<a href='Stores/Edit?Id=" + rowdata.Id + "'id='lnkEditStore' class='btn btn-success btn-block btn-sm' style='cursor:pointer;color:white;'>Edit</a>";
    }
    function deleteStoreLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDeleteStore' class='btn btn-danger btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DeleteStore.call(this)'>Delete</a>";
    }
    function downloadStoreCodeLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDownloadCode' class='btn btn-primary btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DownloadCode.call(this)'>Download Code</a>";
    }
});