$(function () {
    var currentPage = $(".ui-pg-input").val();
    $("#gridEmployee").jqGrid({
        url: 'Employee/GetEmployees',
        datatype: 'json',
        mtype: 'Get',
        colNames: ['Id', 'Email', 'Role', '', ''],
        colModel: [
            { key: true, hidden: true, name: 'Id', index: 'Id', editable: false, sortable: false, width: 200 },
            { key: false, name: 'Email', index: 'Email', editable: false, sortable: false, width: 100, search: false },
            { key: false, name: 'RoleId', index: 'RoleId', editable: true, edittype: 'select', formatter: 'select', editoptions: { value: getAllRoleOptions() }, sortable: false, width: 100, search: false },
            { name: 'Id', index: 'Id', editable: false, sortable: false, width: 30, formatter: editRoleLink },
            { name: 'Id', search: false, index: 'Id', sortable: false, width: 30, formatter: deleteEmployeeLink }
        ],
        //pager: jQuery('#pager'),
        rowNum: -1,
        //rowList: [10, 20, 30, 40],
        viewrecords: true,
        height: '100%',
        width: '100%',
        sortname: 'Email',
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
            ids = $("#gridEmployee").jqGrid('getDataIDs');
            var l = ids.length;
            for (var i = 0; i < l; i++) {
                $("#gridEmployee").jqGrid('editRow', ids[i], true);
            }
            //Here jqGridDemoId is the table id
            if (this.p.datatype === 'json') {
                setTimeout(function () {
                    $('#gridEmployeeUser').trigger("reloadGrid", [{ page: currentPage }]);
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
    function editRoleLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkEditRole' class='btn btn-success btn-block btn-sm' style='cursor:pointer;color:white;' onClick='ChangeRole.call(this)'>Change Role</a>";
    }
    function deleteEmployeeLink(cellValue, options, rowdata, action) {
        return "<a href='#' id='lnkDeleteEmployee' class='btn btn-danger btn-block btn-sm' style='cursor:pointer;color:white;' onClick='DeleteEmployee.call(this)'>Delete</a>";
    }
    function getAllRoleOptions() {
        var roles = { '2': 'Admin', '3': 'Employee' };
        return roles;
    }
});