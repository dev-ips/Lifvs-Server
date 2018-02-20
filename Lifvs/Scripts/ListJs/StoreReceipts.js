$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#storeReceiptsGrid").jqGrid({
        url: '../Sales/GetStoreReceipts?storeId=' + $("#StoreId").val(),
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Receipt Date', 'Amount', 'VAT 1', 'VAT 2', 'Email', 'Total Articles'],
        colModel: [
            { key: true, hidden: false, name: 'Id', index: 'Id', editable: false, width: 20, sortable: true, sorttype: 'text', searchoptions: { sopt: ['cn', 'nc', 'eq', 'ne', 'bw', 'bn', 'ew', 'en'] } },
            {
                key: false, name: 'ReceiptDate', index: 'ReceiptDate', editable: false, sortable: true, sorttype: 'date', width: 40, search: true, formatter: 'date', formatoptions: { srcformat: 'd/m/Y', newformat: 'd/m/Y' },
                searchoptions: {
                    dataInit: function (element) {
                        $(element).datepicker({
                            id: 'receiptDate_datePicker',
                            dateFormat: "dd/mm/yy",
                            showOn: 'focus', autoclose: true
                        });
                    }, sopt: ['cn', 'nc', 'eq', 'ne', 'bw', 'bn', 'ew', 'en']
                }
            },
            { key: false, name: 'Amount', index: 'Amount', editable: false, search: true, sortable: true, sorttype: 'text', searchoptions: { sopt: ['cn', 'nc', 'eq', 'ne', 'bw', 'bn', 'ew', 'en'] }, width: 40 },
            { key: false, name: 'Vat1', index: 'Vat1', editable: false, sortable: true, search: true, sorttype: 'text', searchoptions: { sopt: ['cn', 'nc', 'eq', 'ne', 'bw', 'bn', 'ew', 'en'] }, width: 20 },
            { key: false, name: 'Vat2', index: 'Vat2', editable: false, sortable: true, search: true, sorttype: 'text', searchoptions: { sopt: ['cn', 'nc', 'eq', 'ne', 'bw', 'bn', 'ew', 'en'] }, width: 20 },
            { key: false, name: 'Email', index: 'Email', editable: false, sortable: true, width: 40, search: true, sorttype: 'text', searchoptions: { sopt: ['cn'] } },
            { key: false, name: 'TotalArticles', index: 'TotalArticles', editable: false, sortable: true, width: 40, search: true, sorttype: 'text', searchoptions: { sopt: ['cn'] } },
            //{ name: 'Id', search: false, index: 'Id', sortable: false, width: 20, formatter: deleteStoreInventoryLink }
        ],
        //pager: jQuery('#pager'),
        rowNum: -1,
        //rowList: [10, 20, 30, 40],
        viewrecords: true,
        height: '100%',
        width: '100%',
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
            $('#storeReceiptsGrid').trigger("reloadGrid", [{ page: currentPage }]);
            //$("#storeReceiptsGrid").trigger("reloadGrid");
            //just make all the row to editmode
            //ids = $("#storeReceiptsGrid").jqGrid('getDataIDs');
            //var l = ids.length;
            //for (var i = 0; i < l; i++) {
            //    $("#storeReceiptsGrid").jqGrid('editRow', ids[i], true);
            //}

        },
        autowidth: true
    });
    $('#storeReceiptsGrid').jqGrid('filterToolbar', { stringResult: true, searchOperators: true, autowidth: true });
    //.navGrid('#pager', { edit: false, add: false, del: false, search: false, refresh: false, searchtext: "Search From here", autowidth: true },
    //    {
    //    },
    //    {}, // add options
    //    {}, // delete options
    //    {
    //        multipleSearch: true
    //    });

    function deleteStoreInventoryLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDeleteStoreInventory' class='btn btn-danger btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DeleteStoreInventory.call(this)'>Delete</a>";
    }

});