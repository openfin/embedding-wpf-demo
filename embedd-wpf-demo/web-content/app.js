var headers = [
    { name: 'last', header: 'Last Name' },
    { name: 'first', header: 'First Name' },
    { name: 'pets', header: 'Pets' },
    { name: 'birth date', header: 'Birth Date' },
    { name: 'birth state', header: 'Birth State' },
    { name: 'residence state', header: 'Residence State' },
    { name: 'employed', header: 'Employed' },
    { name: 'income', header: 'Income' },
    { name: 'travel', header: 'Travel' }
];

var grid = new fin.Hypergrid('#json-example', {
    data: [],
    schema: headers,
});

grid.addProperties({
    showRowNumbers: false,
    noDataMessage: "",
    columnAutosizing: false,
    showFilterRow: false
});

fin.desktop.main(function () {
    fin.desktop.InterApplicationBus.subscribe("*",
        "user-data",
        function (message, uuid) {
            grid.behavior.setData(message.data);
        });
});