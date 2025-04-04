import { createRouter, createWebHistory } from 'vue-router';
import Flights from './components/Flights.vue';
import BookFlight from './components/BookFlight.vue';

const routes = [
    { path: '/', component: Flights },
    { path: '/book/:flightId', component: BookFlight, props: true }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

export default router;