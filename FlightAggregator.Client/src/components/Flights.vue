<template>
    <div>
        <h1>Flights</h1>
        <input v-model="filters.airline" placeholder="Airline" />
        <input v-model.number="filters.minPrice" type="number" placeholder="Min Price" />
        <input v-model.number="filters.maxPrice" type="number" placeholder="Max Price" />
        <button @click="fetchFlights">Search</button>
        <ul>
            <li v-for="flight in flights" :key="flight.id">
                {{ flight.airline }} - {{ flight.price }} USD
                <router-link :to="`/book/${flight.id}`">Book</router-link>
            </li>
        </ul>
    </div>
</template>

<script>
import axios from 'axios';
export default {
    data() {
        return {
            flights: [],
            filters: {
                airline: '',
                minPrice: null,
                maxPrice: null
            }
        };
    },
    methods: {
        async fetchFlights() {
            const params = { ...this.filters };
            const response = await axios.get('http://localhost:5000/api/aggregator/flights', { params });
            this.flights = response.data;
        }
    }
};
</script>