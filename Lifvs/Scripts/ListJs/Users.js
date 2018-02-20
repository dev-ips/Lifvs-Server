$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#grid").jqGrid({
        url: 'Users/GetUsers',
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Email', 'Device Type'],
        colModel: [
            { key: true, hidden: true, name: 'Id', index: 'Id', editable: false, sortable: false, width: 200 },
            //{ key: false, name: 'Name', index: 'Name', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'Email', index: 'Email', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'DeviceType', index: 'DeviceType', editable: false, sortable: false, width: 100, search: false },
            //{ key: true, hidden: true, name: 'Type', index: 'Type', editable: false, sortable: false, width: 200 },
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
        //onSelectRow: function (id) {
        //    window.location.href = 'Users/UserInformation?id=' + id;
        //},
        loadComplete: function () {

            //just make all the row to editmode
            //ids = $("#gridEmployee").jqGrid('getDataIDs');
            //var l = ids.length;
            //for (var i = 0; i < l; i++) {
            //    $("#gridEmployee").jqGrid('editRow', ids[i], true);
            //}
            //Here jqGridDemoId is the table id
            if (this.p.datatype === 'json') {
                setTimeout(function () {
                    $('#grid').trigger("reloadGrid", [{ page: currentPage }]);
                }, 50000000);
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
    //$('#gridEmployeeUser').jqGrid('filterToolbar', { stringResult: true, searchOnEnter: false, defaultSearch: 'cn' });
});