const abiertos = new Map();
let desbordamientoAnterior;

export function abrir(dialogo, referencia, id) {
    if (!dialogo?.isConnected || abiertos.has(id)) return;
    const focoAnterior = document.activeElement;
    const cancelar = event => {
        event.preventDefault();
        referencia.invokeMethodAsync('SolicitarCerrarAsync').catch(() => {});
    };
    if (abiertos.size === 0) {
        desbordamientoAnterior = document.body.style.overflow;
        document.body.style.overflow = 'hidden';
    }
    abiertos.set(id, { dialogo, focoAnterior, cancelar });
    dialogo.addEventListener('cancel', cancelar);
    try { dialogo.showModal(); }
    catch (error) { cerrar(id); throw error; }
}

export function cerrar(id) {
    const estado = abiertos.get(id);
    if (!estado) return;
    estado.dialogo.removeEventListener('cancel', estado.cancelar);
    if (estado.dialogo.open) estado.dialogo.close();
    abiertos.delete(id);
    if (abiertos.size === 0) document.body.style.overflow = desbordamientoAnterior ?? '';
    if (estado.focoAnterior?.isConnected) estado.focoAnterior.focus({ preventScroll: true });
}
