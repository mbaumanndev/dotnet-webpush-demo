let notifState = document.getElementById('status-container');

window.addEventListener('load', function () {
    askNotificationPermission().then(result => {
        notifState.innerText = result;
        registerServiceWorker();
    }).catch(reason => {
        notifState.innerText = reason;
    });
});

function registerServiceWorker() {
    return navigator.serviceWorker.register('/sw.js', {scope : '/'}).then(registration => {
        console.log('Service worker is registered', registration);

        return fetch('/api/get_vapid_public_key').then(response => {
            return response.text().then(text => {
                return registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlBase64ToUint8Array(text)
                });
            });
        });
    })
    .then(pushSubscription => {
        console.log('Received subscription', JSON.stringify(pushSubscription));

        const sub = {
            p256dh: base64Encode(pushSubscription.getKey('p256dh')),
            auth: base64Encode(pushSubscription.getKey('auth')),
            endpoint: pushSubscription.endpoint
        }

        console.log("sub", JSON.stringify(sub));

        return fetch('/api/save_user_endpoint', {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: "POST",
            body: JSON.stringify(sub)
        }).then(res => res.json());
    })
    .then(res => {
        console.log('Reponse du serveur', JSON.stringify(res));
    }).catch(err => {
        console.error('Unable to register service worker', err);
    });
}

function askNotificationPermission () {
    return new Promise((resolve, reject) => {
        Notification.requestPermission().then(result => {
            switch (result) {
                case 'denied':
                    reject('La permission n\'a pas été accordée');
                    break;
                case 'default':
                    reject('La réponse a été remise à plus tard');
                    break;
                case 'granted':
                    resolve('Les notifications sont actives');
                    break;
                default :
                    reject('Une erreur est survenue');
            }
        });
    });
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
      .replace(/\-/g, '+')
      .replace(/_/g, '/');
  
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);
  
    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

function base64Encode(arrayBuffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
}
