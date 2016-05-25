var headers = [
    'last',
    'first',
    'pets',
    'birth date',
    'birth state',
    'residence state',
    'employed',
    'income',
    'travel'
];
var grid = new fin.Hypergrid('#json-example', {
    data: [],
    schema: headers,
});

grid.addProperties({
    showRowNumbers:false, 
    noDataMessage: "", 
    columnAutosizing: false
});

fin.desktop.main(function (){
    fin.desktop.InterApplicationBus.subscribe("*",
        "user-data",
        function (message, uuid) {
            grid.behavior.setData(message.data);
    });
});