self.addEventListener("push", e => {
    const data = e.data.json();

    self.registration.showNotification(data.title, {
        body: data.options.message || "Notified",
        icon: data.options.icon || null
    });
});
