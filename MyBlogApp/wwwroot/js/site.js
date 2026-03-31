// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
	const revealElements = document.querySelectorAll('.reveal-on-load');

	if (revealElements.length === 0) {
		return;
	}

	if (!('IntersectionObserver' in window)) {
		revealElements.forEach((element) => element.classList.add('is-visible'));
		return;
	}

	const observer = new IntersectionObserver((entries) => {
		entries.forEach((entry) => {
			if (entry.isIntersecting) {
				entry.target.classList.add('is-visible');
				observer.unobserve(entry.target);
			}
		});
	}, {
		threshold: 0.12,
	});

	revealElements.forEach((element) => observer.observe(element));
});
