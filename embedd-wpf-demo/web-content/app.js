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

fin.desktop.main(async function () {
    const channelName = "user-data";
    const dataChangeTopic = "data-updated";
    const selectionChangeTopic = "selection-changed";

    //fin.InterApplicationBus.subscribe({ uuid: "*" },
    //    "user-data",
    //    function (message, uuid) {
    //        grid.behavior.setData(message.data);
    //    });

    let channelClient = await fin.InterApplicationBus.Channel.connect(channelName);
    channelClient.register(dataChangeTopic, data => {
        console.log('data received');
        grid.behavior.setData(data);
    });


    grid.addEventListener('click', (e) => {
        let selectedValue = getSelectedValue() || '';
        channelClient.dispatch(selectionChangeTopic, selectedValue);
    });
});

function getSelectedValue() {
    let selection = grid.getSelection();

    if (selection.length !== 1)
        return;

    let columns = selection[0];

    if (Object.keys(columns).length !== 1)
        return;

    let values = columns[Object.keys(columns)[0]];

    if (values.length !== 1)
        return;

    return values[0];
}