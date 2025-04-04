<template>
    <div id="app">
        <nav>
            <router-link to="/">Flights</router-link>
        </nav>
        <router-view />
    </div>
</template>

<script>
export default {
    name: "App"
};
</script>

<style>
nav {
    background-color: #42b983;
    padding: 10px;
}
nav a {
    color: white;
    text-decoration: none;
    margin-right: 15px;
}
</style>
