function upsertItem(item) {
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    if (!item.id) {
        throw new Error("Item must have an id");
    }

    var isAccepted = collection.upsertDocument(
        collection.getSelfLink(),
        item,
        function (err, document) {
            if (err) throw err;
            response.setBody(document);
        }
    );

    if (!isAccepted) {
        throw new Error("Stored procedure execution not accepted");
    }
}
